using Microsoft.EntityFrameworkCore;
using Shopizy.ReviewService.Application.Interfaces;
using Shopizy.ReviewService.Domain.Entities;

namespace Shopizy.ReviewService.Infrastructure.Persistence.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly ReviewDbContext _context;

    public WishlistRepository(ReviewDbContext context)
    {
        _context = context;
    }

    public async Task<Wishlist?> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Wishlists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.CustomerId == customerId);
    }

    public async Task AddAsync(Wishlist wishlist)
    {
        await _context.Wishlists.AddAsync(wishlist);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Wishlist wishlist)
    {
        var existingItemIds = await _context.WishlistItems
            .Where(i => i.WishlistId == wishlist.Id)
            .Select(i => i.Id)
            .ToListAsync();

        var currentItemIds = wishlist.Items.Select(i => i.Id).ToHashSet();

        // Mark added or unchanged items
        foreach (var item in wishlist.Items)
        {
            if (!existingItemIds.Contains(item.Id))
            {
                _context.Entry(item).State = EntityState.Added;
            }
            else
            {
                _context.Entry(item).State = EntityState.Unchanged;
            }
        }

        // Find removed items and delete them
        var removedItems = await _context.WishlistItems
            .Where(i => i.WishlistId == wishlist.Id && !currentItemIds.Contains(i.Id))
            .ToListAsync();

        if (removedItems.Count > 0)
        {
            _context.WishlistItems.RemoveRange(removedItems);
        }

        _context.Entry(wishlist).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
