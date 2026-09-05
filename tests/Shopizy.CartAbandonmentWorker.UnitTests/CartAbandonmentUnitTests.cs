using FluentAssertions;
using Shopizy.CartAbandonmentWorker.Domain.Entities;
using Shopizy.CartAbandonmentWorker.Domain.Exceptions;
using Shopizy.CartAbandonmentWorker.Domain.Services;
using Xunit;

namespace Shopizy.CartAbandonmentWorker.UnitTests;

public class CartAbandonmentUnitTests
{
    [Fact]
    public void IsAbandoned_ShouldReturnTrue_WhenInactivityExceedsOrEqualsTwoHours()
    {
        var now = DateTime.UtcNow;
        var lastActivity = now.AddHours(-2);

        var isAbandoned = AbandonmentPolicy.IsAbandoned(lastActivity, itemCount: 1, now);

        isAbandoned.Should().BeTrue();
    }

    [Fact]
    public void IsAbandoned_ShouldReturnFalse_WhenInactivityLessThanTwoHours()
    {
        var now = DateTime.UtcNow;
        var lastActivity = now.AddHours(-1.9);

        var isAbandoned = AbandonmentPolicy.IsAbandoned(lastActivity, itemCount: 2, now);

        isAbandoned.Should().BeFalse();
    }

    [Fact]
    public void IsAbandoned_ShouldReturnFalse_WhenCartHasNoItems()
    {
        var now = DateTime.UtcNow;
        var lastActivity = now.AddHours(-5);

        var isAbandoned = AbandonmentPolicy.IsAbandoned(lastActivity, itemCount: 0, now);

        isAbandoned.Should().BeFalse();
    }

    [Fact]
    public void IsInCooldown_ShouldReturnTrue_WhenLastDispatchedLessThan24HoursAgo()
    {
        var now = DateTime.UtcNow;
        var lastDispatched = now.AddHours(-23.9);

        var inCooldown = AbandonmentPolicy.IsInCooldown(lastDispatched, now);

        inCooldown.Should().BeTrue();
    }

    [Fact]
    public void IsInCooldown_ShouldReturnFalse_WhenLastDispatched24HoursOrMoreAgo()
    {
        var now = DateTime.UtcNow;
        var lastDispatched = now.AddHours(-24);

        var inCooldown = AbandonmentPolicy.IsInCooldown(lastDispatched, now);

        inCooldown.Should().BeFalse();
    }

    [Fact]
    public void IsInCooldown_ShouldReturnFalse_WhenNeverDispatched()
    {
        var now = DateTime.UtcNow;

        var inCooldown = AbandonmentPolicy.IsInCooldown(null, now);

        inCooldown.Should().BeFalse();
    }

    [Fact]
    public void FormatRecoveryUrl_ShouldReturnCorrectUrl()
    {
        var baseUrl = "https://shopizy.com/";
        var token = "abc123token";

        var url = AbandonmentPolicy.FormatRecoveryUrl(baseUrl, token);

        url.Should().Be("https://shopizy.com/cart/restore/abc123token");
    }

    [Fact]
    public void AbandonedCartRecord_Create_ShouldSucceedWithValidParameters()
    {
        var cartId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var email = "Customer@Example.com";
        var total = 99.50m;
        var itemsJson = "[{\"productId\":\"123\",\"quantity\":2}]";
        var lastActivity = DateTime.UtcNow.AddHours(-3);

        var record = AbandonedCartRecord.Create(cartId, customerId, email, total, itemsJson, lastActivity);

        record.CartId.Should().Be(cartId);
        record.CustomerId.Should().Be(customerId);
        record.CustomerEmail.Should().Be("customer@example.com");
        record.CartTotal.Should().Be(total);
        record.ItemsJson.Should().Be(itemsJson);
        record.RecoveryToken.Should().NotBeNullOrWhiteSpace();
        record.IsRestored.Should().BeFalse();
        record.RestoredAtUtc.Should().BeNull();
    }

    [Fact]
    public void AbandonedCartRecord_Create_ShouldThrow_WhenCartIdEmpty()
    {
        var act = () => AbandonedCartRecord.Create(Guid.Empty, Guid.NewGuid(), "test@example.com", 10m, "[]", DateTime.UtcNow);

        act.Should().Throw<CartAbandonmentDomainException>()
            .WithMessage("*CartId cannot be empty.*");
    }

    [Fact]
    public void AbandonedCartRecord_Create_ShouldThrow_WhenCustomerIdEmpty()
    {
        var act = () => AbandonedCartRecord.Create(Guid.NewGuid(), Guid.Empty, "test@example.com", 10m, "[]", DateTime.UtcNow);

        act.Should().Throw<CartAbandonmentDomainException>()
            .WithMessage("*CustomerId cannot be empty.*");
    }

    [Fact]
    public void AbandonedCartRecord_Create_ShouldThrow_WhenEmailEmpty()
    {
        var act = () => AbandonedCartRecord.Create(Guid.NewGuid(), Guid.NewGuid(), "", 10m, "[]", DateTime.UtcNow);

        act.Should().Throw<CartAbandonmentDomainException>()
            .WithMessage("*Customer email cannot be empty.*");
    }

    [Fact]
    public void AbandonedCartRecord_MarkAsRestored_ShouldUpdateStatus()
    {
        var record = AbandonedCartRecord.Create(Guid.NewGuid(), Guid.NewGuid(), "test@example.com", 10m, "[]", DateTime.UtcNow);

        record.MarkAsRestored();

        record.IsRestored.Should().BeTrue();
        record.RestoredAtUtc.Should().NotBeNull();
    }
}
