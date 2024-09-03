using ErrorOr;
using MediatR;
using Shopizy.Ordering.API.Aggregates;
using Shopizy.Ordering.API.Aggregates.ValueObjects;
using Shopizy.Ordering.API.Errors;
using Shopizy.Ordering.API.Persistence;

namespace Shopizy.Ordering.API.Services.Queries.GetOrder;

public class GetOrderQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrderQuery, ErrorOr<Order>>
{
    private readonly IOrderRepository _orderRepository = orderRepository;

    public async Task<ErrorOr<Order>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        Order? order = await _orderRepository.GetOrderByIdAsync(OrderId.Create(request.OrderId));

        if (order is null)
        {
            return CustomErrors.Order.OrderNotFound;
        }

        return order;
    }
}
