using Shopizy.Catelog.API.Aggregates.Products;
using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;
using Shopizy.Domain.Models.Specifications;

namespace Shopizy.Catelog.API.Persistence.Products.Specifications;

internal class ProductsByIdsSpec : Specification<Product>
{
    public ProductsByIdsSpec(IList<ProductId> ids) : base(product => ids.Contains(product.Id))
    {
        AddInclude(p => p.ProductImages);
    }
}
