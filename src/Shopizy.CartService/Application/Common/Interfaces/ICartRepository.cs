using Shopizy.CartService.Domain.Entities;

namespace Shopizy.CartService.Application.Common.Interfaces;

/// <summary>Port: persistence contract for the Cart aggregate (Redis-backed).</summary>
public interface ICartRepository
{
    Task<Cart?> GetAsync(string cartId, CancellationToken cancellationToken = default);
    Task SaveAsync(Cart cart, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task DeleteAsync(string cartId, CancellationToken cancellationToken = default);
}
