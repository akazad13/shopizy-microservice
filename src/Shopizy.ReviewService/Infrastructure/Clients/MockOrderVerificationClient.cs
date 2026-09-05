using Shopizy.ReviewService.Application.Interfaces;

namespace Shopizy.ReviewService.Infrastructure.Clients;

public class MockOrderVerificationClient : IOrderVerificationClient
{
    private readonly HashSet<(Guid CustomerId, Guid ProductId)> _deliveredPurchases = new();

    public void RegisterDeliveredPurchase(Guid customerId, Guid productId)
    {
        _deliveredPurchases.Add((customerId, productId));
    }

    public Task<bool> IsDeliveredOrderAsync(Guid customerId, Guid productId, Guid? orderId = null)
    {
        // If orderId is provided or recorded in delivered purchases, return true
        if (orderId.HasValue && orderId.Value != Guid.Empty)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(_deliveredPurchases.Contains((customerId, productId)));
    }
}
