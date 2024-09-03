using Shopizy.Catelog.API.Aggregates.Products;
using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;

namespace Shopizy.Catelog.API.Persistence.Products;

public interface IProductRepository
{
    Task<List<Product>> GetProductsAsync();
    Task<Product?> GetProductByIdAsync(ProductId id);
    Task<List<Product>> GetProductsByIdsAsync(IList<ProductId> ids);
    Task<bool> IsProductExistAsync(ProductId id);
    Task AddAsync(Product product);
    void Update(Product product);
    void Remove(Product product);
    Task<int> Commit(CancellationToken cancellationToken);
}
