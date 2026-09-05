namespace Shopizy.ReviewService.Application.Interfaces;

public interface IOrderVerificationClient
{
    Task<bool> IsDeliveredOrderAsync(Guid customerId, Guid productId, Guid? orderId = null);
}
