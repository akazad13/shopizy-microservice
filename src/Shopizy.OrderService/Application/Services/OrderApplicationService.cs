using Shopizy.OrderService.Application.Contracts;
using Shopizy.OrderService.Application.Interfaces;
using Shopizy.OrderService.Domain.Entities;
using Shopizy.OrderService.Domain.Enums;
using Shopizy.OrderService.Domain.Exceptions;
using Shopizy.OrderService.Domain.ValueObjects;

namespace Shopizy.OrderService.Application.Services;

public sealed class OrderApplicationService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IInventoryRepository _inventoryRepo;

    public OrderApplicationService(IOrderRepository orderRepo, IInventoryRepository inventoryRepo)
    {
        _orderRepo = orderRepo;
        _inventoryRepo = inventoryRepo;
    }

    public async Task<OrderResponse> CreateOrderAsync(Guid customerId, CreateOrderRequest request, CancellationToken ct = default)
    {
        if (request.Items is null || request.Items.Count == 0)
            throw new OrderDomainException("Order.EmptyItems", "Cannot create order with no items.");

        // Step 1: Atomic stock reservations
        var reservedItems = new List<(InventoryItem item, int qty)>();

        try
        {
            foreach (var reqItem in request.Items)
            {
                var inventory = await _inventoryRepo.GetByVariantIdAsync(reqItem.VariantId, ct)
                    ?? throw new OrderDomainException("Inventory.NotFound", $"Inventory record for variant {reqItem.VariantId} not found.");

                inventory.ReserveStock(reqItem.Quantity);
                reservedItems.Add((inventory, reqItem.Quantity));
            }

            // Save all inventory updates
            foreach (var (inv, _) in reservedItems)
            {
                await _inventoryRepo.UpdateAsync(inv, ct);
            }
        }
        catch
        {
            // Rollback any reservations in this batch
            foreach (var (inv, qty) in reservedItems)
            {
                inv.ReleaseReservation(qty);
                await _inventoryRepo.UpdateAsync(inv, ct);
            }
            throw;
        }

        // Step 2: Create Order
        var orderId = Guid.NewGuid();
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{orderId.ToString("N")[..8].ToUpperInvariant()}";
        var address = new ShippingAddress(
            request.ShippingAddress.FullName,
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.PostalCode,
            request.ShippingAddress.Country);

        var order = Order.Create(orderId, orderNumber, customerId, address);

        foreach (var item in request.Items)
        {
            var unitPrice = Money.Create(item.UnitPrice.Amount, item.UnitPrice.Currency);
            order.AddItem(item.ProductId, item.VariantId, item.ProductName, item.VariantSku, item.Quantity, unitPrice);
        }

        await _orderRepo.AddAsync(order, ct);

        return ToResponse(order);
    }

    public async Task<OrderResponse?> GetOrderAsync(Guid orderId, Guid? requestingCustomerId, bool isAdmin, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct);
        if (order is null) return null;

        // Multi-tenant isolation: customer can only view own order
        if (!isAdmin && order.CustomerId != requestingCustomerId)
            return null;

        return ToResponse(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> ListOrdersAsync(Guid? requestingCustomerId, bool isAdmin, CancellationToken ct = default)
    {
        IReadOnlyList<Order> orders;
        if (isAdmin && !requestingCustomerId.HasValue)
        {
            orders = await _orderRepo.GetAllAsync(ct);
        }
        else if (requestingCustomerId.HasValue)
        {
            orders = await _orderRepo.GetByCustomerIdAsync(requestingCustomerId.Value, ct);
        }
        else
        {
            orders = [];
        }

        return orders.Select(ToResponse).ToList();
    }

    public async Task<OrderResponse> PayOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct)
            ?? throw new OrderDomainException("Order.NotFound", $"Order {orderId} not found.");

        order.MarkAsPaid();

        // Commit reservations
        foreach (var item in order.Items)
        {
            var inv = await _inventoryRepo.GetByVariantIdAsync(item.VariantId, ct);
            if (inv is not null)
            {
                inv.CommitReservation(item.Quantity);
                await _inventoryRepo.UpdateAsync(inv, ct);
            }
        }

        await _orderRepo.UpdateAsync(order, ct);
        return ToResponse(order);
    }

    public async Task<OrderResponse> CancelOrderAsync(Guid orderId, Guid? requestingCustomerId, bool isAdmin, string reason, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct)
            ?? throw new OrderDomainException("Order.NotFound", $"Order {orderId} not found.");

        if (!isAdmin && order.CustomerId != requestingCustomerId)
            throw new OrderDomainException("Order.Unauthorized", "Cannot cancel another customer's order.");

        var previousStatus = order.Status;
        order.Cancel(reason);

        // Restock / release reservation
        foreach (var item in order.Items)
        {
            var inv = await _inventoryRepo.GetByVariantIdAsync(item.VariantId, ct);
            if (inv is not null)
            {
                if (previousStatus == OrderStatus.PendingPayment)
                {
                    inv.ReleaseReservation(item.Quantity);
                }
                else if (previousStatus == OrderStatus.Processing)
                {
                    inv.Restock(item.Quantity);
                }
                await _inventoryRepo.UpdateAsync(inv, ct);
            }
        }

        await _orderRepo.UpdateAsync(order, ct);
        return ToResponse(order);
    }

    public async Task<OrderResponse> ExpireOrderAsync(Guid orderId, DateTimeOffset? asOf = null, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct)
            ?? throw new OrderDomainException("Order.NotFound", $"Order {orderId} not found.");

        if (order.ExpireIfUnpaid(asOf))
        {
            // Release reserved stock back to available
            foreach (var item in order.Items)
            {
                var inv = await _inventoryRepo.GetByVariantIdAsync(item.VariantId, ct);
                if (inv is not null)
                {
                    inv.ReleaseReservation(item.Quantity);
                    await _inventoryRepo.UpdateAsync(inv, ct);
                }
            }
            await _orderRepo.UpdateAsync(order, ct);
        }

        return ToResponse(order);
    }

    public async Task<int> ExpireAllPendingOrdersAsync(DateTimeOffset asOf, CancellationToken ct = default)
    {
        var expired = await _orderRepo.GetExpiredPendingOrdersAsync(asOf, ct);
        int count = 0;

        foreach (var order in expired)
        {
            if (order.ExpireIfUnpaid(asOf))
            {
                foreach (var item in order.Items)
                {
                    var inv = await _inventoryRepo.GetByVariantIdAsync(item.VariantId, ct);
                    if (inv is not null)
                    {
                        inv.ReleaseReservation(item.Quantity);
                        await _inventoryRepo.UpdateAsync(inv, ct);
                    }
                }
                await _orderRepo.UpdateAsync(order, ct);
                count++;
            }
        }

        return count;
    }

    public async Task<OrderResponse> ShipOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct)
            ?? throw new OrderDomainException("Order.NotFound", $"Order {orderId} not found.");

        order.MarkAsShipped();
        await _orderRepo.UpdateAsync(order, ct);
        return ToResponse(order);
    }

    public async Task<OrderResponse> DeliverOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct)
            ?? throw new OrderDomainException("Order.NotFound", $"Order {orderId} not found.");

        order.MarkAsDelivered();
        await _orderRepo.UpdateAsync(order, ct);
        return ToResponse(order);
    }

    public async Task<InventoryResponse?> GetInventoryAsync(Guid variantId, CancellationToken ct = default)
    {
        var inv = await _inventoryRepo.GetByVariantIdAsync(variantId, ct);
        return inv is null ? null : new InventoryResponse(inv.VariantId, inv.AvailableStock, inv.ReservedStock);
    }

    public async Task<InventoryResponse> AdjustInventoryAsync(Guid variantId, int quantity, CancellationToken ct = default)
    {
        var inv = await _inventoryRepo.GetByVariantIdAsync(variantId, ct);
        if (inv is null)
        {
            inv = new InventoryItem(variantId, Math.Max(0, quantity));
            await _inventoryRepo.AddAsync(inv, ct);
        }
        else
        {
            inv.Restock(quantity);
            await _inventoryRepo.UpdateAsync(inv, ct);
        }

        return new InventoryResponse(inv.VariantId, inv.AvailableStock, inv.ReservedStock);
    }

    public static OrderResponse ToResponse(Order order)
    {
        var items = order.Items.Select(i => new OrderItemResponse(
            i.Id,
            i.ProductId,
            i.VariantId,
            i.ProductName,
            i.VariantSku,
            i.Quantity,
            new MoneyDto(i.UnitPrice.Amount, i.UnitPrice.Currency),
            new MoneyDto(i.LineTotal.Amount, i.LineTotal.Currency))).ToList();

        var addr = new ShippingAddressDto(
            order.ShippingAddress.FullName,
            order.ShippingAddress.Street,
            order.ShippingAddress.City,
            order.ShippingAddress.State,
            order.ShippingAddress.PostalCode,
            order.ShippingAddress.Country);

        return new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.Status.ToString(),
            addr,
            items,
            new MoneyDto(order.TotalAmount.Amount, order.TotalAmount.Currency),
            order.CreatedAtUtc,
            order.ExpiresAtUtc,
            order.PaidAtUtc,
            order.ShippedAtUtc,
            order.DeliveredAtUtc,
            order.CancelledAtUtc,
            order.CancellationReason);
    }
}
