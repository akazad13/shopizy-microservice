using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;
using Shopizy.Catelog.API.Errors;
using Shopizy.Catelog.API.ExternalServices.MediaUploader;
using Shopizy.Catelog.API.Persistence.Products;

namespace Shopizy.Catelog.API.Services.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler(IProductRepository productRepository, IMediaUploader mediaUploader)
        : IRequestHandler<DeleteProductCommand, ErrorOr<Success>>
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IMediaUploader _mediaUploader = mediaUploader;

    public async Task<ErrorOr<Success>> Handle(DeleteProductCommand cmd, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetProductByIdAsync(ProductId.Create(cmd.ProductId));

        if (product is null)
            return CustomErrors.Product.ProductNotFound;

        // Delete product image from media

        _productRepository.Remove(product);

        if (await _productRepository.Commit(cancellationToken) <= 0)
            return CustomErrors.Product.ProductNotDeleted;

        return Result.Success;

    }
}