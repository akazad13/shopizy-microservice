using FluentAssertions;
using Shopizy.ShippingService.Domain.Enums;
using Shopizy.ShippingService.Domain.Entities;
using Shopizy.ShippingService.Domain.Services;
using Xunit;

namespace Shopizy.ShippingService.UnitTests;

public class CarrierRateCalculatorUnitTests
{
    [Fact]
    public void CalculateRates_SubtotalOver75_WaivesUspsGroundShipping()
    {
        var rates = CarrierRateCalculator.CalculateRates(85m, 2.0m, "US");

        var usps = rates.First(r => r.Carrier == "USPS" && r.ServiceLevel == "Ground Advantage");
        usps.Cost.Should().Be(0.00m);
        usps.Description.Should().Contain("Free Ground Shipping");
    }

    [Fact]
    public void CalculateRates_SubtotalUnder75_ChargesStandardGroundFee()
    {
        var rates = CarrierRateCalculator.CalculateRates(50m, 2.0m, "US");

        var usps = rates.First(r => r.Carrier == "USPS" && r.ServiceLevel == "Ground Advantage");
        usps.Cost.Should().BeGreaterThan(0.00m);
    }

    [Fact]
    public void CalculateRates_ReturnsAllMajorCarriers()
    {
        var rates = CarrierRateCalculator.CalculateRates(100m, 1.5m, "US");

        rates.Should().Contain(r => r.Carrier == "USPS");
        rates.Should().Contain(r => r.Carrier == "UPS");
        rates.Should().Contain(r => r.Carrier == "FedEx");
        rates.Should().Contain(r => r.Carrier == "DHL");
    }

    [Fact]
    public void Shipment_Creation_InitializesLabelCreatedMilestone()
    {
        var shipment = Shipment.Create(
            Guid.NewGuid(), Guid.NewGuid(), "USPS", "Ground Advantage", 1.5m, "123 Main St", "60601");

        shipment.Status.Should().Be(ShipmentStatus.LabelCreated);
        shipment.TrackingNumber.Should().StartWith("trk_usps_");
        shipment.Milestones.Should().HaveCount(1);
        shipment.Milestones[0].Status.Should().Be(ShipmentStatus.LabelCreated);
    }
}
