using Shopizy.CartAbandonmentWorker.Domain.Exceptions;

namespace Shopizy.CartAbandonmentWorker.Domain.Entities;

public class AbandonedCartRecord
{
    public Guid Id { get; private set; }
    public Guid CartId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public decimal CartTotal { get; private set; }
    public string ItemsJson { get; private set; } = "[]";
    public DateTime LastActivityUtc { get; private set; }
    public string RecoveryToken { get; private set; } = string.Empty;
    public DateTime DispatchedAtUtc { get; private set; }
    public bool IsRestored { get; private set; }
    public DateTime? RestoredAtUtc { get; private set; }

    private AbandonedCartRecord() { }

    public static AbandonedCartRecord Create(
        Guid cartId,
        Guid customerId,
        string customerEmail,
        decimal cartTotal,
        string itemsJson,
        DateTime lastActivityUtc)
    {
        if (cartId == Guid.Empty)
            throw new CartAbandonmentDomainException("INVALID_CART_ID", "CartId cannot be empty.");

        if (customerId == Guid.Empty)
            throw new CartAbandonmentDomainException("INVALID_CUSTOMER_ID", "CustomerId cannot be empty.");

        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new CartAbandonmentDomainException("EMPTY_EMAIL", "Customer email cannot be empty.");

        return new AbandonedCartRecord
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            CustomerId = customerId,
            CustomerEmail = customerEmail.Trim().ToLowerInvariant(),
            CartTotal = cartTotal,
            ItemsJson = string.IsNullOrWhiteSpace(itemsJson) ? "[]" : itemsJson,
            LastActivityUtc = lastActivityUtc,
            RecoveryToken = Guid.NewGuid().ToString("N"),
            DispatchedAtUtc = DateTime.UtcNow,
            IsRestored = false
        };
    }

    public void MarkAsRestored()
    {
        IsRestored = true;
        RestoredAtUtc = DateTime.UtcNow;
    }
}
