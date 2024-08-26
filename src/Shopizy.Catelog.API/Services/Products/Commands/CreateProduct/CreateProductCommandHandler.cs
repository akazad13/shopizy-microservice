using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Categories.ValueObjects;
using Shopizy.Catelog.API.Aggregates.Products;
using Shopizy.Catelog.API.Errors;
using Shopizy.Catelog.API.Persistence.Products;
using Shopizy.Domain.Models.ValueObjects;

namespace Shopizy.Catelog.API.Services.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(IProductRepository productRepository)
        : IRequestHandler<CreateProductCommand, ErrorOr<Product>>
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<ErrorOr<Product>> Handle(CreateProductCommand cmd, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            cmd.Name,
            cmd.Description,
            CategoryId.Create(cmd.CategoryId),
            cmd.Sku,
            Price.CreateNew(cmd.UnitPrice, cmd.Currency),
            cmd.Discount,
            cmd.Brand,
            cmd.Barcode,
            cmd.Tags, ""
        );

        await _productRepository.AddAsync(product);

        if (await _productRepository.Commit(cancellationToken) <= 0)
            return CustomErrors.Product.ProductNotCreated;

        return product;

    }
}