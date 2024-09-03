using Mapster;
using Shopizy.Cart.API.Aggregates;
using Shopizy.Cart.API.Aggregates.Entities;
using Shopizy.Cart.API.ApiContracts;
using Shopizy.Cart.API.Services.Commands.AddProductToCart;
using Shopizy.Cart.API.Services.Commands.CreateCartWithFirstProduct;
using Shopizy.Cart.API.Services.Commands.RemoveProductsFromCart;
using Shopizy.Cart.API.Services.Commands.UpdateProductQuantity;
using Shopizy.Cart.API.Services.Queries.GetCart;

namespace Shopizy.Cart.API.Mapping;

public class CartMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<
                (Guid UserId, CreateCartWithFirstProductRequest request),
                CreateCartWithFirstProductCommand
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest, src => src.request);

        config
            .NewConfig<
                (Guid UserId, Guid CartId, AddProductToCartRequest request),
                AddProductToCartCommand
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.CartId, src => src.CartId)
            .Map(dest => dest, src => src.request);

        config
            .NewConfig<
                (Guid UserId, Guid CartId, UpdateProductQuantityRequest request),
                UpdateProductQuantityCommand
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.CartId, src => src.CartId)
            .Map(dest => dest, src => src.request);

        config
            .NewConfig<
                (Guid UserId, Guid CartId, RemoveProductFromCartRequest request),
                RemoveProductFromCartCommand
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.CartId, src => src.CartId)
            .Map(dest => dest, src => src.request);

        config.NewConfig<Guid, GetCartQuery>().MapWith(userId => new GetCartQuery(userId));

        config
            .NewConfig<CustomerCart, CartResponse>()
            .Map(dest => dest.CartId, src => src.Id.Value)
            .Map(dest => dest.CustomerId, src => src.CustomerId.Value)
            .Map(dest => dest.CartItems, src => src.CartItems);

        config
            .NewConfig<CartItem, CartItemResponse>()
            .Map(dest => dest.CartItemId, src => src.Id.Value)
            .Map(dest => dest.ProductId, src => src.ProductId.Value)
            .Map(dest => dest.Quantity, src => src.Quantity);
        //.Map(dest => dest.Product, src => src.Product)
        //.Map(
        //    dest => dest.Product.ProductImages,
        //    src =>
        //        src.Product.ProductImages == null
        //            ? null
        //            : src.Product.ProductImages.Select(pi => pi.ImageUrl)
        //);
    }
}
