using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Products;
using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;
using Shopizy.Catelog.API.Persistence.Products;

namespace Shopizy.Catelog.API.Services.Products.Queries.GetProduct;

public class GetProductQueryHandler(IProductRepository productRepository) : IRequestHandler<GetProductQuery, ErrorOr<Product?>>
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<ErrorOr<Product?>> Handle(GetProductQuery query, CancellationToken cancellationToken)
    {
        return await _productRepository.GetProductByIdAsync(ProductId.Create(query.ProductId));
    }
}