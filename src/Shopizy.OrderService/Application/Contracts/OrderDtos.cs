using Shopizy.OrderService.Domain.Enums;

namespace Shopizy.OrderService.Application.Contracts;

public sealed record MoneyDto(decimal Amount, string Currency = "USD");

public sealed record ShippingAddressDto(
    string FullName,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed record CreateOrderItemDto(
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string VariantSku,
    int Quantity,
    MoneyDto UnitPrice);

public sealed record CreateOrderRequest(
    List<CreateOrderItemDto> Items,
    ShippingAddressDto ShippingAddress);

public sealed record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string VariantSku,
    int Quantity,
    MoneyDto UnitPrice,
    MoneyDto LineTotal);

public sealed record OrderResponse(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string Status,
    ShippingAddressDto ShippingAddress,
    List<OrderItemResponse> Items,
    MoneyDto TotalAmount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset? PaidAtUtc,
    DateTimeOffset? ShippedAtUtc,
    DateTimeOffset? DeliveredAtUtc,
    DateTimeOffset? CancelledAtUtc,
    string? CancellationReason);

public sealed record InventoryResponse(
    Guid VariantId,
    int AvailableStock,
    int ReservedStock);

public sealed record AdjustInventoryRequest(int Quantity);
