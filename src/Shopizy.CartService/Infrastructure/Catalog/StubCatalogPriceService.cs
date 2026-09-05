using Shopizy.CartService.Application.Common.Interfaces;

namespace Shopizy.CartService.Infrastructure.Catalog;

/// <summary>
/// Stub implementation of ICatalogPriceService.
/// In production this would call the Catalog Service via HTTP/gRPC.
/// For E2E tests this is replaced by a controllable mock.
/// </summary>
public sealed class StubCatalogPriceService : ICatalogPriceService
{
    // In-process price overrides (used by tests to inject price changes)
    private readonly Dictionary<Guid, decimal> _priceOverrides = [];

    public void SetPrice(Guid variantId, decimal price) => _priceOverrides[variantId] = price;

    public Task<decimal?> GetCurrentPriceAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        decimal? price = _priceOverrides.TryGetValue(variantId, out var p) ? p : null;
        return Task.FromResult(price);
    }
}
