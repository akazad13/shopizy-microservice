using ErrorOr;
using MediatR;
using Shopizy.Ordering.API.Aggregates;
using Shopizy.Ordering.API.Persistence;

namespace Shopizy.Ordering.API.Services.Queries.ListOrders;

public class ListOrdersQueryHandler(IOrderRepository orderRepository) : IRequestHandler<ListOrdersQuery, ErrorOr<List<Order>>>
{
    private readonly IOrderRepository _orderRepository = orderRepository;

    public async Task<ErrorOr<List<Order>>> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetOrdersAsync();
        return orders;
    }
}
