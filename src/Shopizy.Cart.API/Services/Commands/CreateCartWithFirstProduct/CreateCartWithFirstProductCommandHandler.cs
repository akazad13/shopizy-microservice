using ErrorOr;
using MediatR;
using Shopizy.Cart.API.Aggregates;
using Shopizy.Cart.API.Aggregates.Entities;
using Shopizy.Cart.API.Aggregates.ValueObjects;
using Shopizy.Cart.API.Errors;
using Shopizy.Cart.API.Persistence;

namespace Shopizy.Cart.API.Services.Commands.CreateCartWithFirstProduct;

public class CreateCartWithFirstProductCommandHandler(ICartRepository cartRepository)
        : IRequestHandler<CreateCartWithFirstProductCommand, ErrorOr<CustomerCart>>
{
    private readonly ICartRepository _cartRepository = cartRepository;
    public async Task<ErrorOr<CustomerCart>> Handle(CreateCartWithFirstProductCommand cmd, CancellationToken cancellationToken)
    {
        //var product = await _productRepository.IsProductExistAsync(ProductId.Create(cmd.ProductId));

        //if (!product)
        //    return CustomErrors.Product.ProductNotFound;

        var cart = CustomerCart.Create(CustomerId.Create(cmd.UserId));
        cart.AddCartItem(CartItem.Create(ProductId.Create(cmd.ProductId)));

        await _cartRepository.AddAsync(cart);

        if (await _cartRepository.Commit(cancellationToken) <= 0)
            return CustomErrors.Cart.CartNotCreated;
        return (await _cartRepository.GetCartByUserIdAsync(CustomerId.Create(cmd.UserId)))!;
    }
}