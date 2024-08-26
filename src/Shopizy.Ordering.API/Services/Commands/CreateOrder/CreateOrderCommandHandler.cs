using ErrorOr;
using MediatR;
using Shopizy.Domain.Models.ValueObjects;
using Shopizy.Ordering.API.Aggregates;
using Shopizy.Ordering.API.Aggregates.Entities;
using Shopizy.Ordering.API.Aggregates.ValueObjects;
using Shopizy.Ordering.API.Errors;
using Shopizy.Ordering.API.Persistence;

namespace Shopizy.Ordering.API.Services.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository
) : IRequestHandler<CreateOrderCommand, ErrorOr<Order>>
{
    private readonly IOrderRepository _orderRepository = orderRepository;

    public async Task<ErrorOr<Order>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        //var products = await _productRepository.GetProductsByIdsAsync(
        //    request.OrderItems.Select(x => ProductId.Create(x.ProductId)).ToList()
        //);
        //if (products.Count == 0)
        //    return CustomErrors.Product.ProductNotFound;

        // foreach( var product in products)
        // {
        //     if(product.StockQuantity < request.OrderItems.First(p => p.ProductId == items.Id.Value).Quantity)
        //     {
        //         return "Prduct is not available";
        //     }
        // }

        // Replace this with actual product
        var products = new List<OrderItem> {
            OrderItem.Create("Item 1", "", Price.CreateNew(100, 0), 2, 5),
            OrderItem.Create("Item 2", "", Price.CreateNew(100, 0), 2, 5)
        };

        var order = Order.Create(
            customerId: CustomerId.Create(request.UserId),
            promoCode: request.PromoCode,
            deliveryCharge: Price.CreateNew(
                request.DeliveryChargeAmount,
                request.DeliveryChargeCurrency
            ),
            shippingAddress: Address.CreateNew(
                street: request.ShippingAddress.Street,
                city: request.ShippingAddress.City,
                state: request.ShippingAddress.State,
                country: request.ShippingAddress.Country,
                zipCode: request.ShippingAddress.ZipCode
            ),
            orderItems: products
        );

        await _orderRepository.AddAsync(order);
        if (await _orderRepository.Commit(cancellationToken) <= 0)
            return CustomErrors.Order.OrderNotCreated;

        return order;
    }
}
