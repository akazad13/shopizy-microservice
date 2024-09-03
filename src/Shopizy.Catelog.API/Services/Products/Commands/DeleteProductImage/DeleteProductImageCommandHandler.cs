using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Products.ValueObjects;
using Shopizy.Catelog.API.Errors;
using Shopizy.Catelog.API.ExternalServices.MediaUploader;
using Shopizy.Catelog.API.Persistence.Products;

namespace Shopizy.Catelog.API.Services.Products.Commands.DeleteProductImage;

public class DeleteProductImageCommandHandler(IProductRepository productRepository, IMediaUploader mediaUploader)
        : IRequestHandler<DeleteProductImageCommand, ErrorOr<Success>>
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IMediaUploader _mediaUploader = mediaUploader;

    public async Task<ErrorOr<Success>> Handle(DeleteProductImageCommand cmd, CancellationToken cancellationToken)
    {
        Aggregates.Products.Product? product = await _productRepository.GetProductByIdAsync(ProductId.Create(cmd.ProductId));

        if (product is null)
        {
            return CustomErrors.Product.ProductNotFound;
        }

        Aggregates.Products.Entities.ProductImage? prodImage = product.ProductImages.FirstOrDefault(pi => pi.Id == ProductImageId.Create(cmd.ImageId));

        if (prodImage is null)
        {
            return CustomErrors.Product.ProductImageNotFound;
        }

        ErrorOr<Success> res = await _mediaUploader.DeletePhotoAsync(prodImage.PublicId);

        if (!res.IsError)
        {
            product.RemoveProductImage(prodImage);

            _productRepository.Update(product);

            if (await _productRepository.Commit(cancellationToken) <= 0)
            {
                return CustomErrors.Product.ProductImageNotAdded;
            }

            return Result.Success;
        }
        return res.Errors;
    }
}
