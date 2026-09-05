namespace Shopizy.CartService.Application.Common.Interfaces;

/// <summary>Fetches current live catalog prices for price discrepancy checks.</summary>
public interface ICatalogPriceService
{
    /// <summary>Returns the current catalog price for a variant, or null if unavailable.</summary>
    Task<decimal?> GetCurrentPriceAsync(Guid variantId, CancellationToken cancellationToken = default);
}
