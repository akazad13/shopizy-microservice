using Microsoft.EntityFrameworkCore;
using Shopizy.Cart.API.Aggregates;
using Shopizy.Cart.API.Aggregates.ValueObjects;

namespace Shopizy.Cart.API.Persistence;

public class CartRepository(CartDbContext dbContext) : ICartRepository
{
    private readonly CartDbContext _dbContext = dbContext;

    public IQueryable<CustomerCart> GetCarts()
    {
        return _dbContext.Carts.AsNoTracking();
    }
    public Task<CustomerCart?> GetCartByIdAsync(CartId id)
    {
        return _dbContext.Carts.FirstOrDefaultAsync(c => c.Id == id);
    }
    public Task<CustomerCart?> GetCartByUserIdAsync(CustomerId id)
    {
        return _dbContext.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.CustomerId == id);
    }
    public async Task AddAsync(CustomerCart cart)
    {
        _ = await _dbContext.Carts.AddAsync(cart);
    }
    public void Update(CustomerCart cart)
    {
        _ = _dbContext.Update(cart);
    }
    public void Remove(CustomerCart cart)
    {
        _ = _dbContext.Remove(cart);
    }
    public Task<int> Commit(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
