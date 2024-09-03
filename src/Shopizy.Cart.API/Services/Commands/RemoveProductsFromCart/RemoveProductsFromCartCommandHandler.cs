using ErrorOr;
using MediatR;
using Shopizy.Cart.API.Aggregates.ValueObjects;
using Shopizy.Cart.API.Errors;
using Shopizy.Cart.API.Persistence;

namespace Shopizy.Cart.API.Services.Commands.RemoveProductsFromCart;

public class RemoveProductFromCartCommandHandler(ICartRepository cartRepository)
        : IRequestHandler<RemoveProductFromCartCommand, ErrorOr<Success>>
{
    private readonly ICartRepository _cartRepository = cartRepository;
    public async Task<ErrorOr<Success>> Handle(RemoveProductFromCartCommand cmd, CancellationToken cancellationToken)
    {
        Aggregates.CustomerCart? cart = await _cartRepository.GetCartByIdAsync(CartId.Create(cmd.CartId));

        if (cart is null)
        {
            return CustomErrors.Cart.CartNotFound;
        }

        foreach (Guid productid in cmd.ProductIds)
        {
            Aggregates.Entities.CartItem? cartItem = cart.CartItems.FirstOrDefault(li => li.ProductId.Value == productid);
            if (cartItem is not null)
            {
                cart.RemoveCartItem(cartItem);
            }
        }
        _cartRepository.Update(cart);

        if (await _cartRepository.Commit(cancellationToken) <= 0)
        {
            return CustomErrors.Cart.CartPrductNotRemoved;
        }

        return Result.Success;

    }
}
