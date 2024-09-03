using Ardalis.GuardClauses;
using Mapster;
using Shopizy.Catelog.API.Aggregates.Products;
using Shopizy.Catelog.API.Aggregates.Products.Entities;
using Shopizy.Catelog.API.ApiContracts.Product;
using Shopizy.Catelog.API.Services.Products.Commands.CreateProduct;
using Shopizy.Catelog.API.Services.Products.Commands.DeleteProduct;
using Shopizy.Catelog.API.Services.Products.Commands.DeleteProductImage;
using Shopizy.Catelog.API.Services.Products.Commands.UpdateProduct;
using Shopizy.Catelog.API.Services.Products.Queries.GetProduct;
using Shopizy.Catelog.API.Services.Products.Queries.ProductAvailability;

namespace Shopizy.Catelog.API.Mapping;

public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        _ = Guard.Against.Null(config);

        _ = config
            .NewConfig<(Guid UserId, CreateProductRequest request), CreateProductCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest, src => src.request);

        _ = config
            .NewConfig<
                (Guid UserId, Guid productId, UpdateProductRequest request),
                UpdateProductCommand
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.ProductId, src => src.productId)
            .Map(dest => dest, src => src.request);

        _ = config
            .NewConfig<(Guid UserId, Guid ProductId), DeleteProductCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.ProductId, src => src.ProductId);

        _ = config.NewConfig<Guid, GetProductQuery>().MapWith(src => new GetProductQuery(src));
        _ = config.NewConfig<Guid, ProductAvailabilityQuery>().MapWith(src => new ProductAvailabilityQuery(src));

        _ = config
            .NewConfig<Product, ProductResponse>()
            .Map(dest => dest.ProductId, src => src.Id.Value)
            .Map(dest => dest.CategoryId, src => src.CategoryId.Value)
            .Map(dest => dest.Sku, src => src.SKU)
            .Map(dest => dest.Price, src => src.UnitPrice.Amount.ToString());

        _ = config
            .NewConfig<ProductImage, ProductImageResponse>()
            .Map(dest => dest.ProductImageId, src => src.Id.Value);

        _ = config
            .NewConfig<(Guid UserId, Guid ProductId, Guid ImageId), DeleteProductImageCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.ImageId, src => src.ImageId);
    }
}
