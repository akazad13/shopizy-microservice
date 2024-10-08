using ErrorOr;
using MediatR;
using Shopizy.Cart.API.Aggregates;
using Shopizy.Cart.API.Aggregates.Entities;
using Shopizy.Cart.API.Aggregates.ValueObjects;
using Shopizy.Cart.API.Errors;
using Shopizy.Cart.API.Persistence;
using Shopizy.Dapr.QueryServices;
using Shopizy.Dapr.QueryServices.Products;

namespace Shopizy.Cart.API.Services.Commands.CreateCartWithFirstProduct;

public class CreateCartWithFirstProductCommandHandler(ICartRepository cartRepository, IQueryService<IsProductExistQuery, bool> productExistQuery)
        : IRequestHandler<CreateCartWithFirstProductCommand, ErrorOr<CustomerCart>>
{
    private readonly ICartRepository _cartRepository = cartRepository;
    private readonly IQueryService<IsProductExistQuery, bool> _productExistQuery = productExistQuery;
    public async Task<ErrorOr<CustomerCart>> Handle(CreateCartWithFirstProductCommand cmd, CancellationToken cancellationToken)
    {
        bool productExits = await _productExistQuery.QueryAsync(new IsProductExistQuery(cmd.ProductId));

        if (!productExits)
        {
            return CustomErrors.Product.ProductNotFound;
        }

        var cart = CustomerCart.Create(CustomerId.Create(cmd.UserId));
        cart.AddCartItem(CartItem.Create(ProductId.Create(cmd.ProductId)));

        await _cartRepository.AddAsync(cart);

        if (await _cartRepository.Commit(cancellationToken) <= 0)
        {
            return CustomErrors.Cart.CartNotCreated;
        }

        return (await _cartRepository.GetCartByUserIdAsync(CustomerId.Create(cmd.UserId)))!;
    }
}
