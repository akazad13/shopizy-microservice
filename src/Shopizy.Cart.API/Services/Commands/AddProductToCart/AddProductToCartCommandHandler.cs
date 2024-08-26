using ErrorOr;
using MediatR;
using Shopizy.Cart.API.Aggregates;
using Shopizy.Cart.API.Aggregates.Entities;
using Shopizy.Cart.API.Aggregates.ValueObjects;
using Shopizy.Cart.API.Errors;
using Shopizy.Cart.API.Persistence;

namespace Shopizy.Cart.API.Services.Commands.AddProductToCart;

public class AddProductToCartCommandHandler(ICartRepository cartRepository)
        : IRequestHandler<AddProductToCartCommand, ErrorOr<CustomerCart>>
{
    private readonly ICartRepository _cartRepository = cartRepository;

    public async Task<ErrorOr<CustomerCart>> Handle(AddProductToCartCommand cmd, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetCartByIdAsync(CartId.Create(cmd.CartId));

        if (cart is null)
            return CustomErrors.Cart.CartNotFound;

        if (cart.CartItems.Any(li => li.ProductId == ProductId.Create(cmd.ProductId)))
            return CustomErrors.Cart.ProductAlreadyExistInCart;

        //var product = await _productRepository.IsProductExistAsync(ProductId.Create(cmd.ProductId));

        //if (!product)
        //    return CustomErrors.Product.ProductNotFound;

        cart.AddCartItem(CartItem.Create(ProductId.Create(cmd.ProductId)));

        _cartRepository.Update(cart);

        if (await _cartRepository.Commit(cancellationToken) <= 0)
            return CustomErrors.Cart.CartPrductNotAdded;
        return (await _cartRepository.GetCartByUserIdAsync(CustomerId.Create(cmd.UserId)))!;
    }
}