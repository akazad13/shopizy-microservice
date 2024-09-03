using ErrorOr;
using MediatR;
using Shopizy.Cart.API.Aggregates.ValueObjects;
using Shopizy.Cart.API.Errors;
using Shopizy.Cart.API.Persistence;

namespace Shopizy.Cart.API.Services.Commands.UpdateProductQuantity;

public class UpdateProductQuantityCommandHandler(ICartRepository cartRepository)
        : IRequestHandler<UpdateProductQuantityCommand, ErrorOr<Success>>
{
    private readonly ICartRepository _cartRepository = cartRepository;
    public async Task<ErrorOr<Success>> Handle(UpdateProductQuantityCommand cmd, CancellationToken cancellationToken)
    {
        Aggregates.CustomerCart? cart = await _cartRepository.GetCartByIdAsync(CartId.Create(cmd.CartId));

        if (cart is null)
        {
            return CustomErrors.Cart.CartNotFound;
        }

        cart.UpdateCartItem(ProductId.Create(cmd.ProductId), cmd.Quantity);

        _cartRepository.Update(cart);

        if (await _cartRepository.Commit(cancellationToken) <= 0)
        {
            return CustomErrors.Cart.CartPrductNotAdded;
        }

        return Result.Success;

    }
}
