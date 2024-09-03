using Microsoft.EntityFrameworkCore;
using Shopizy.Catelog.API.Aggregates.Products;
using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;
using Shopizy.Catelog.API.Persistence.Products.Specifications;
using Shopizy.Domain.Models.Specifications;

namespace Shopizy.Catelog.API.Persistence.Products;

public class ProductRepository(CatelogDbContext dbContext) : IProductRepository
{
    private readonly CatelogDbContext _dbContext = dbContext;

    public IQueryable<Product> GetProductsAsync()
    {
        return _dbContext.Products.AsNoTracking();
    }
    public Task<Product?> GetProductByIdAsync(ProductId id)
    {
        return _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
    }
    public IQueryable<Product> GetProductsByIdsAsync(IList<ProductId> ids)
    {
        return ApplySpec(new ProductsByIdsSpec(ids));
    }
    public Task<bool> IsProductExistAsync(ProductId id)
    {
        return _dbContext.Products.AnyAsync(p => p.Id == id);
    }
    public async Task AddAsync(Product product)
    {
        await _dbContext.Products.AddAsync(product);
    }
    public void Update(Product product)
    {
        _dbContext.Update(product);
    }

    public void Remove(Product product)
    {
        _dbContext.Remove(product);
    }

    public Task<int> Commit(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Product> ApplySpec(Specification<Product> spec)
    {
        return SpecificationEvaluator.GetQuery(_dbContext.Products, spec);
    }
}
