using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shopizy.Catelog.API.Aggregates.Products;
using Shopizy.Catelog.API.Persistence.Products;

namespace Shopizy.Catelog.API.Services.Products.Queries.ListProducts;

public class ListProductQueryHandler(IProductRepository productRepository) : IRequestHandler<ListProductQuery, ErrorOr<List<Product>?>>
{
    private readonly IProductRepository _productRepository = productRepository;
    public async Task<ErrorOr<List<Product>?>> Handle(ListProductQuery query, CancellationToken cancellationToken)
    {
        return await _productRepository.GetProductsAsync().ToListAsync(cancellationToken: cancellationToken);
    }
}
