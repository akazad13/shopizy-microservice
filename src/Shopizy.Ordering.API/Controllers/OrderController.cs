using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Ordering.API.ApiContracts;
using Shopizy.Ordering.API.Services.Commands.CancelOrder;
using Shopizy.Ordering.API.Services.Commands.CreateOrder;
using Shopizy.Ordering.API.Services.Queries.GetOrder;
using Shopizy.Ordering.API.Services.Queries.ListOrders;

namespace Shopizy.Ordering.API.Controllers;

[Route("api/v1.0/users/{userId:guid}/orders")]
public class OrderController(ISender mediator, IMapper mapper) : ApiController
{
    private readonly ISender _mediator = mediator;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<IActionResult> GetOrdersAsync(Guid UserId)
    {
        ListOrdersQuery query = _mapper.Map<ListOrdersQuery>(UserId);
        ErrorOr.ErrorOr<List<Aggregates.Order>> result = await _mediator.Send(query);

        return result.Match(Product => Ok(_mapper.Map<List<OrderResponse>>(Product)), Problem);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrderAsync(Guid userId, Guid orderId)
    {
        GetOrderQuery query = _mapper.Map<GetOrderQuery>((userId, orderId));
        ErrorOr.ErrorOr<Aggregates.Order> result = await _mediator.Send(query);

        return result.Match(order => Ok(_mapper.Map<OrderResponse?>(order)), Problem);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync(Guid userId, CreateOrderRequest request)
    {
        CreateOrderCommand command = _mapper.Map<CreateOrderCommand>((userId, request));
        ErrorOr.ErrorOr<Aggregates.Order> result = await _mediator.Send(command);

        return result.Match(order => Ok(_mapper.Map<OrderResponse>(order)), Problem);
    }

    [HttpDelete("{orderId:guid}")]
    public async Task<IActionResult> CancelOrderAsync(
        Guid userId,
        Guid orderId,
        CancelOrderRequest request
    )
    {
        CancelOrderCommand command = _mapper.Map<CancelOrderCommand>((userId, orderId, request));
        ErrorOr.ErrorOr<ErrorOr.Success> result = await _mediator.Send(command);

        return result.Match(success => Ok(success), Problem);
    }
}

