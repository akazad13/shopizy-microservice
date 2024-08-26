using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Persistence.Products;
using Shopizy.Catelog.API.Errors;
using Shopizy.Catelog.API.Aggregates.Products;
using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;
using Shopizy.Catelog.API.Aggregates.Categories.ValueObjects;
using Shopizy.Domain.Models.ValueObjects;

namespace Shopizy.Catelog.API.Services.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler(IProductRepository productRepository)
        : IRequestHandler<UpdateProductCommand, ErrorOr<Product>>
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<ErrorOr<Product>> Handle(UpdateProductCommand cmd, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetProductByIdAsync(ProductId.Create(cmd.ProductId));

        if (product is null)
            return CustomErrors.Product.ProductNotFound;

        product.Update(
            cmd.Name,
            cmd.Description,
            CategoryId.Create(cmd.CategoryId),
            cmd.Sku,
            Price.CreateNew(cmd.UnitPrice, cmd.Currency),
            cmd.Discount,
            cmd.Brand,
            cmd.Barcode,
            cmd.Tags
        );

        _productRepository.Update(product);

        if (await _productRepository.Commit(cancellationToken) <= 0)
            return CustomErrors.Product.ProductNotUpdated;

        return product;

    }
}