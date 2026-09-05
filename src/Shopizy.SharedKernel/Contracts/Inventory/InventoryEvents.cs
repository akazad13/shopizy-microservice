namespace Shopizy.SharedKernel.Contracts.Inventory;

public sealed record ReservedItemDto(Guid ProductId, string Sku, int Quantity);

public sealed record StockReservedIntegrationEvent(
    Guid OrderId,
    DateTime ReservedAtUtc,
    IReadOnlyList<ReservedItemDto> Items);

public sealed record StockReservationFailedIntegrationEvent(
    Guid OrderId,
    string Sku,
    int RequestedQuantity,
    int AvailableQuantity,
    DateTime FailedAtUtc);

public sealed record StockRestockedIntegrationEvent(
    Guid OrderId,
    DateTime RestockedAtUtc,
    IReadOnlyList<ReservedItemDto> Items);
