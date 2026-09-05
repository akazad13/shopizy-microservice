namespace Shopizy.ShippingService.Domain.Services;

public sealed record ShippingRateQuote(
    string Carrier,
    string ServiceLevel,
    decimal Cost,
    string Currency,
    int EstimatedDays,
    string Description);

public static class CarrierRateCalculator
{
    public const decimal FreeShippingThreshold = 75.00m;

    public static IReadOnlyList<ShippingRateQuote> CalculateRates(decimal subtotal, decimal weightKg, string country)
    {
        bool qualifiesForFreeShipping = subtotal >= FreeShippingThreshold;
        var quotes = new List<ShippingRateQuote>();

        // 1. USPS Ground Advantage
        decimal uspsGround = qualifiesForFreeShipping ? 0.00m : Math.Round(4.99m + (weightKg * 1.20m), 2);
        quotes.Add(new ShippingRateQuote(
            "USPS",
            "Ground Advantage",
            uspsGround,
            "USD",
            qualifiesForFreeShipping ? 3 : 4,
            qualifiesForFreeShipping ? "Free Ground Shipping (Order over $75)" : "Standard Ground"));

        // 2. UPS Ground
        decimal upsGround = Math.Round(7.99m + (weightKg * 1.50m), 2);
        quotes.Add(new ShippingRateQuote("UPS", "Ground", upsGround, "USD", 3, "Day-definite ground delivery"));

        // 3. FedEx 2-Day Express
        decimal fedexExpress = Math.Round(14.99m + (weightKg * 2.50m), 2);
        quotes.Add(new ShippingRateQuote("FedEx", "2-Day", fedexExpress, "USD", 2, "Second business day delivery"));

        // 4. DHL Express Worldwide / Overnight
        decimal dhlOvernight = Math.Round(24.99m + (weightKg * 4.00m), 2);
        quotes.Add(new ShippingRateQuote("DHL", "Express", dhlOvernight, "USD", 1, "Next-day priority delivery"));

        return quotes;
    }
}
