using Shopizy.Domain.Models.Specifications;
using Shopizy.Ordering.API.Aggregates;
using Shopizy.Ordering.API.Aggregates.ValueObjects;

namespace Shopizy.Ordering.API.Persistence.Specifications;

public class OrderByIdSpec : Specification<Order>
{
    public OrderByIdSpec(OrderId id) : base(order => order.Id == id)
    {
        AddInclude(order => order.OrderItems);
    }
}

