using FluentAssertions;
using Shopizy.SharedKernel.Domain;
using Xunit;

namespace Shopizy.SharedKernel.UnitTests.Domain;

public class ValueObjectTests
{
    private sealed class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void Equals_WhenValuesHaveIdenticalComponents_ReturnsTrue()
    {
        var m1 = new Money(100.50m, "USD");
        var m2 = new Money(100.50m, "USD");

        (m1 == m2).Should().BeTrue();
        m1.Equals(m2).Should().BeTrue();
        m1.GetHashCode().Should().Be(m2.GetHashCode());
    }

    [Fact]
    public void Equals_WhenValuesHaveDifferentComponents_ReturnsFalse()
    {
        var m1 = new Money(100.50m, "USD");
        var m2 = new Money(200.00m, "USD");
        var m3 = new Money(100.50m, "EUR");

        (m1 != m2).Should().BeTrue();
        m1.Equals(m2).Should().BeFalse();
        m1.Equals(m3).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenComparedWithNull_ReturnsFalse()
    {
        var m1 = new Money(100.50m, "USD");

        m1.Equals(null).Should().BeFalse();
        (m1 == null).Should().BeFalse();
        (null == m1).Should().BeFalse();
    }
}
