namespace Shopizy.SharedKernel.Contracts.Cart;

public sealed record CartItemSummaryDto(Guid ProductId, string Sku, string ProductName, int Quantity, decimal UnitPrice);

public sealed record CartAbandonedIntegrationEvent(
    Guid CartId,
    Guid CustomerId,
    string CustomerEmail,
    string CustomerName,
    decimal TotalAmount,
    DateTime AbandonedAtUtc,
    IReadOnlyList<CartItemSummaryDto> Items);
