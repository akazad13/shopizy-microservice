namespace Shopizy.LoyaltyService.Domain.Services;

public static class LoyaltyCalculator
{
    // $1 spent = 1 point earned
    public static int CalculatePointsEarned(decimal orderAmount)
    {
        if (orderAmount <= 0) return 0;
        return (int)Math.Floor(orderAmount);
    }

    // 100 points = $1.00 discount
    public static decimal CalculateDiscount(int pointsToRedeem)
    {
        if (pointsToRedeem <= 0) return 0m;
        return Math.Round((decimal)pointsToRedeem / 100m, 2, MidpointRounding.AwayFromZero);
    }
}
