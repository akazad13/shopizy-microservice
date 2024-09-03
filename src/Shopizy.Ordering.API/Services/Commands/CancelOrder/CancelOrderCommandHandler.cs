using ErrorOr;
using MediatR;
using Shopizy.Ordering.API.Aggregates.ValueObjects;
using Shopizy.Ordering.API.Errors;
using Shopizy.Ordering.API.Persistence;

namespace Shopizy.Ordering.API.Services.Commands.CancelOrder;

public class CancelOrderCommandHandler(IOrderRepository orderRepository) : IRequestHandler<CancelOrderCommand, ErrorOr<Success>>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    public async Task<ErrorOr<Success>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetOrderByIdAsync(OrderId.Create(request.OrderId));
        if (order is null)
        {
            return CustomErrors.Order.OrderNotFound;
        }

        order.CancelOrder(request.Reason);

        _orderRepository.Update(order);

        if (await _orderRepository.Commit(cancellationToken) <= 0)
        {
            return CustomErrors.Order.OrderNotCancelled;
        }

        return Result.Success;
    }
}
