using Mapster;
using Shopizy.Domain.Models.Enums;
using Shopizy.Ordering.API.Aggregates;
using Shopizy.Ordering.API.Aggregates.Entities;
using Shopizy.Ordering.API.ApiContracts;
using Shopizy.Ordering.API.Services.Commands.CancelOrder;
using Shopizy.Ordering.API.Services.Commands.CreateOrder;
using Shopizy.Ordering.API.Services.Queries.GetOrder;
using Shopizy.Ordering.API.Services.Queries.ListOrders;

namespace Shopizy.Ordering.API.Mappings;

public class OrderMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<(Guid UserId, CreateOrderRequest request), CreateOrderCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest, src => src.request)
            .Map(dest => dest.DeliveryChargeAmount, src => src.request.DeliveryCharge.Amount)
            .Map(
                dest => dest.DeliveryChargeCurrency,
                src => (Currency)src.request.DeliveryCharge.Currency
            );
        config
            .NewConfig<(Guid UserId, Guid OrderId), GetOrderQuery>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.OrderId, src => src.OrderId);

        config
            .NewConfig<Order, OrderResponse>()
            .Map(dest => dest.OrderId, src => src.Id.Value)
            .Map(dest => dest.CustomerId, src => src.CustomerId.Value)
            .Map(dest => dest, src => src)
            .Map(dest => dest.OrderStatus, src => src.OrderStatus.ToString())
            .Map(dest => dest.PaymentStatus, src => src.PaymentStatus.ToString());

        config.NewConfig<Guid, ListOrdersQuery>().MapWith(userId => new ListOrdersQuery(userId));

        config
            .NewConfig<OrderItem, OrderItemResponse>()
            .Map(dest => dest.OrderItemId, src => src.Id.Value);

        config
            .NewConfig<
                (Guid UserId, Guid OrderId, CancelOrderRequest request),
                CancelOrderCommand
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.OrderId, src => src.OrderId)
            .Map(dest => dest.Reason, src => src.request.Reason);
    }
}
