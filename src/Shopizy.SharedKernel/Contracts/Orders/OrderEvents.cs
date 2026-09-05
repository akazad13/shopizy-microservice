namespace Shopizy.SharedKernel.Contracts.Orders;

public sealed record OrderItemDto(
    Guid ProductId,
    string Sku,
    int Quantity,
    decimal UnitPrice);

public sealed record OrderPlacedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    DateTime PlacedAtUtc,
    DateTime ExpiresAtUtc,
    IReadOnlyList<OrderItemDto> Items);

public sealed record OrderCancelledIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    string Reason,
    DateTime CancelledAtUtc);

public sealed record OrderExpiredIntegrationEvent(
    Guid OrderId,
    DateTime ExpiredAtUtc,
    string Reason);
