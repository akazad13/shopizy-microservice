using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;
using Shopizy.Catelog.API.Persistence.Products;

namespace Shopizy.Catelog.API.Services.Products.Queries.ProductAvailability;

public class ProductAvailabilityQueryHandler(IProductRepository productRepository) : IRequestHandler<ProductAvailabilityQuery, ErrorOr<bool>>
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<ErrorOr<bool>> Handle(ProductAvailabilityQuery query, CancellationToken cancellationToken)
    {
        return await _productRepository.IsProductExistAsync(ProductId.Create(query.ProductId));
    }
}
