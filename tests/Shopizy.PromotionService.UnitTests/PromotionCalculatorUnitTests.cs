using FluentAssertions;
using Shopizy.PromotionService.Application.Contracts;
using Shopizy.PromotionService.Domain.Entities;
using Shopizy.PromotionService.Domain.Enums;
using Shopizy.PromotionService.Infrastructure.Calculators;
using Xunit;

namespace Shopizy.PromotionService.UnitTests;

public class PromotionCalculatorUnitTests
{
    private readonly DefaultPromotionCalculator _calculator = new();

    [Fact]
    public void Evaluate_PercentageDiscount_AppliesCapCorrectly()
    {
        var campaign = PromotionCampaign.Create(
            Guid.NewGuid(), "SAVE20", "20% off", DiscountType.Percentage, 20m, null, 50m, null, null,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(5));

        var req = new ApplyPromotionRequest("SAVE20", 300m, "USD", new List<CartItemDto>
        {
            new(Guid.NewGuid(), "Item 1", "General", 300m, 1)
        });

        var result = _calculator.Evaluate(campaign, req);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(50m); // 20% of 300 is 60, but capped at 50!
    }

    [Fact]
    public void Evaluate_FixedAmount_AppliesDiscount()
    {
        var campaign = PromotionCampaign.Create(
            Guid.NewGuid(), "FLAT15", "$15 off", DiscountType.FixedAmount, 15m, 50m, null, null, null,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(5));

        var req = new ApplyPromotionRequest("FLAT15", 80m, "USD", new List<CartItemDto>
        {
            new(Guid.NewGuid(), "Item 1", "General", 80m, 1)
        });

        var result = _calculator.Evaluate(campaign, req);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(15m);
    }

    [Fact]
    public void Evaluate_MinimumSpendNotMet_ReturnsInvalid()
    {
        var campaign = PromotionCampaign.Create(
            Guid.NewGuid(), "TIER100", "$15 off on $100", DiscountType.FixedAmount, 15m, 100m, null, null, null,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(5));

        var req = new ApplyPromotionRequest("TIER100", 75m, "USD", new List<CartItemDto>
        {
            new(Guid.NewGuid(), "Item 1", "General", 75m, 1)
        });

        var result = _calculator.Evaluate(campaign, req);

        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("Minimum subtotal");
    }

    [Fact]
    public void Evaluate_Bogo_DiscountsLowestPriceItem()
    {
        var campaign = PromotionCampaign.Create(
            Guid.NewGuid(), "BOGO", "Buy 2 Get 1 Free", DiscountType.Bogo, 1m, null, null, null, null,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(5));

        var req = new ApplyPromotionRequest("BOGO", 100m, "USD", new List<CartItemDto>
        {
            new(Guid.NewGuid(), "Item A", "General", 50m, 1),
            new(Guid.NewGuid(), "Item B", "General", 30m, 1),
            new(Guid.NewGuid(), "Item C", "General", 20m, 1) // Lowest item is $20
        });

        var result = _calculator.Evaluate(campaign, req);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(20m);
    }
}
