using System.Text.Json;
using FluentAssertions;
using Shopizy.SharedKernel.Contracts.Cart;
using Shopizy.SharedKernel.Contracts.Catalog;
using Shopizy.SharedKernel.Contracts.Inventory;
using Shopizy.SharedKernel.Contracts.Orders;
using Shopizy.SharedKernel.Contracts.Payments;
using Shopizy.SharedKernel.Contracts.Shipping;
using Xunit;

namespace Shopizy.SharedKernel.UnitTests.Contracts;

public class IntegrationEventSerializationTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    [Fact]
    public void OrderPlacedIntegrationEvent_SerializesAndDeserializesAccurately()
    {
        var original = new OrderPlacedIntegrationEvent(
            OrderId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            TotalAmount: 299.95m,
            Currency: "USD",
            PlacedAtUtc: DateTime.UtcNow,
            ExpiresAtUtc: DateTime.UtcNow.AddMinutes(15),
            Items:
            [
                new OrderItemDto(Guid.NewGuid(), "TSHIRT-RED-L", 2, 49.99m),
                new OrderItemDto(Guid.NewGuid(), "JEANS-BLU-32", 1, 199.97m)
            ]
        );

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<OrderPlacedIntegrationEvent>(json, _jsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.OrderId.Should().Be(original.OrderId);
        deserialized.CustomerId.Should().Be(original.CustomerId);
        deserialized.TotalAmount.Should().Be(original.TotalAmount);
        deserialized.Currency.Should().Be(original.Currency);
        deserialized.Items.Should().HaveCount(2);
        deserialized.Items[0].Sku.Should().Be("TSHIRT-RED-L");
        deserialized.Items[0].UnitPrice.Should().Be(49.99m);
    }

    [Fact]
    public void PaymentCompletedIntegrationEvent_SerializesAndDeserializesAccurately()
    {
        var original = new PaymentCompletedIntegrationEvent(
            PaymentId: Guid.NewGuid(),
            OrderId: Guid.NewGuid(),
            TransactionId: "txn_stripe_abc123",
            AmountPaid: 299.95m,
            Currency: "USD",
            PaidAtUtc: DateTime.UtcNow
        );

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<PaymentCompletedIntegrationEvent>(json, _jsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.TransactionId.Should().Be("txn_stripe_abc123");
        deserialized.AmountPaid.Should().Be(299.95m);
    }

    [Fact]
    public void ShipmentDispatchedIntegrationEvent_SerializesAndDeserializesAccurately()
    {
        var original = new ShipmentDispatchedIntegrationEvent(
            ShipmentId: Guid.NewGuid(),
            OrderId: Guid.NewGuid(),
            Carrier: "FedEx",
            TrackingNumber: "FX-99887766",
            TrackingUrl: "https://fedex.com/track/FX-99887766",
            DispatchedAtUtc: DateTime.UtcNow,
            EstimatedDeliveryUtc: DateTime.UtcNow.AddDays(2)
        );

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<ShipmentDispatchedIntegrationEvent>(json, _jsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.Carrier.Should().Be("FedEx");
        deserialized.TrackingNumber.Should().Be("FX-99887766");
    }

    [Fact]
    public void CartAbandonedIntegrationEvent_SerializesAndDeserializesAccurately()
    {
        var original = new CartAbandonedIntegrationEvent(
            CartId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            CustomerEmail: "shopper@example.com",
            CustomerName: "John Doe",
            TotalAmount: 149.50m,
            AbandonedAtUtc: DateTime.UtcNow,
            Items:
            [
                new CartItemSummaryDto(Guid.NewGuid(), "SKU-ABC", "Running Shoe", 1, 149.50m)
            ]
        );

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<CartAbandonedIntegrationEvent>(json, _jsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.CustomerEmail.Should().Be("shopper@example.com");
        deserialized.Items.Should().HaveCount(1);
    }

    [Fact]
    public void ProductPriceChangedIntegrationEvent_SerializesAndDeserializesAccurately()
    {
        var original = new ProductPriceChangedIntegrationEvent(
            ProductId: Guid.NewGuid(),
            Sku: "IPHONE-16-PRO",
            OldPrice: 1099.00m,
            NewPrice: 999.00m,
            Currency: "USD",
            ChangedAtUtc: DateTime.UtcNow
        );

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<ProductPriceChangedIntegrationEvent>(json, _jsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.OldPrice.Should().Be(1099.00m);
        deserialized.NewPrice.Should().Be(999.00m);
    }
}
