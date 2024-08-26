using ErrorOr;
using MediatR;
using Shopizy.Cart.API.Aggregates;
using Shopizy.Cart.API.Aggregates.ValueObjects;
using Shopizy.Cart.API.Persistence;

namespace Shopizy.Cart.API.Services.Queries.GetCart;

public class GetCartQueryHandler(ICartRepository cartRepository) : IRequestHandler<GetCartQuery, ErrorOr<CustomerCart?>>
{
    private readonly ICartRepository _cartRepository = cartRepository;
    public async Task<ErrorOr<CustomerCart?>> Handle(GetCartQuery query, CancellationToken cancellationToken)
    {
        return await _cartRepository.GetCartByUserIdAsync(CustomerId.Create(query.UserId));
    }
}