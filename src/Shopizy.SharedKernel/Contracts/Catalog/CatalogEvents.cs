namespace Shopizy.SharedKernel.Contracts.Catalog;

public sealed record ProductPriceChangedIntegrationEvent(
    Guid ProductId,
    string Sku,
    decimal OldPrice,
    decimal NewPrice,
    string Currency,
    DateTime ChangedAtUtc);
