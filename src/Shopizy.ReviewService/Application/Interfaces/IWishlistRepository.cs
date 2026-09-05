using Shopizy.ReviewService.Domain.Entities;

namespace Shopizy.ReviewService.Application.Interfaces;

public interface IWishlistRepository
{
    Task<Wishlist?> GetByCustomerIdAsync(Guid customerId);
    Task AddAsync(Wishlist wishlist);
    Task UpdateAsync(Wishlist wishlist);
}
