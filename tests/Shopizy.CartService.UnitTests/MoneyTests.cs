using FluentAssertions;
using Shopizy.CartService.Domain.ValueObjects;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.CartService.UnitTests;

public sealed class MoneyTests
{
    [Fact]
    public void Create_NegativeAmount_ThrowsDomainException()
    {
        var act = () => Money.Create(-1m);
        act.Should().Throw<DomainException>().WithMessage("*negative*");
    }

    [Fact]
    public void Create_InvalidCurrencyCode_ThrowsDomainException()
    {
        var act = () => Money.Create(10m, "US");
        act.Should().Throw<DomainException>().WithMessage("*3-letter*");
    }

    [Fact]
    public void Create_ValidAmount_ReturnsMoneyObject()
    {
        var m = Money.Create(19.99m, "USD");
        m.Amount.Should().Be(19.99m);
        m.Currency.Should().Be("USD");
    }

    [Fact]
    public void Multiply_ReturnsCorrectLineTotal()
    {
        var m = Money.Create(25.50m);
        var result = m.Multiply(3);
        result.Amount.Should().Be(76.50m);
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSummedAmount()
    {
        var a = Money.Create(10m);
        var b = Money.Create(20m);
        a.Add(b).Amount.Should().Be(30m);
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsDomainException()
    {
        var usd = Money.Create(10m, "USD");
        var eur = Money.Create(10m, "EUR");
        var act = () => usd.Add(eur);
        act.Should().Throw<DomainException>().WithMessage("*Cannot add*");
    }

    [Fact]
    public void Zero_ReturnsZeroAmount()
    {
        var z = Money.Zero();
        z.Amount.Should().Be(0m);
        z.Currency.Should().Be("USD");
    }
}
