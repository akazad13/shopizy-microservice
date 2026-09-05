using FluentAssertions;
using Shopizy.LoyaltyService.Domain.Entities;
using Shopizy.LoyaltyService.Domain.Enums;
using Shopizy.LoyaltyService.Domain.Exceptions;
using Shopizy.LoyaltyService.Domain.Services;
using Xunit;

namespace Shopizy.LoyaltyService.UnitTests;

public class LoyaltyUnitTests
{
    [Theory]
    [InlineData(150.00, 150)]
    [InlineData(199.99, 199)]
    [InlineData(49.49, 49)]
    [InlineData(0, 0)]
    [InlineData(-10, 0)]
    public void LoyaltyCalculator_CalculatePointsEarned_RoundsDownToWholeDollar(decimal amount, int expectedPoints)
    {
        var points = LoyaltyCalculator.CalculatePointsEarned(amount);
        points.Should().Be(expectedPoints);
    }

    [Theory]
    [InlineData(100, 1.00)]
    [InlineData(500, 5.00)]
    [InlineData(250, 2.50)]
    [InlineData(0, 0)]
    public void LoyaltyCalculator_CalculateDiscount_ConvertsAt100PointsPerDollar(int points, decimal expectedDiscount)
    {
        var discount = LoyaltyCalculator.CalculateDiscount(points);
        discount.Should().Be(expectedDiscount);
    }

    [Fact]
    public void RedeemPoints_ExceedingCurrentBalance_ThrowsLoyaltyDomainException()
    {
        var account = LoyaltyAccount.Create(Guid.NewGuid());
        account.AccruePoints(150, Guid.NewGuid(), "Initial Order");

        var act = () => account.RedeemPoints(200, Guid.NewGuid(), "Order checkout");

        act.Should().Throw<LoyaltyDomainException>()
            .WithMessage("*Cannot redeem 200 points. Current balance is 150*");
    }

    [Fact]
    public void GiftCard_DeductBalance_TransitionsFromActiveToDepleted()
    {
        var card = GiftCard.Create(50.00m, "USD");
        card.Status.Should().Be(GiftCardStatus.Active);
        card.CurrentBalance.Should().Be(50.00m);

        // Partial deduction
        card.DeductBalance(30.00m);
        card.Status.Should().Be(GiftCardStatus.Active);
        card.CurrentBalance.Should().Be(20.00m);

        // Full remaining deduction
        card.DeductBalance(20.00m);
        card.Status.Should().Be(GiftCardStatus.Depleted);
        card.CurrentBalance.Should().Be(0.00m);
    }

    [Fact]
    public void GiftCard_DeductBalance_ExceedingAvailable_ThrowsLoyaltyDomainException()
    {
        var card = GiftCard.Create(25.00m, "USD");

        var act = () => card.DeductBalance(30.00m);

        act.Should().Throw<LoyaltyDomainException>()
            .WithMessage("*Cannot deduct 30*Remaining balance is 25*");
    }
}
