using FluentAssertions;
using Shopizy.OrderService.Domain.Exceptions;
using Shopizy.OrderService.Domain.ValueObjects;

namespace Shopizy.OrderService.UnitTests;

public sealed class MoneyTests
{
    [Fact]
    public void Create_NegativeAmount_ThrowsDomainException()
    {
        var act = () => Money.Create(-5m);
        act.Should().Throw<OrderDomainException>().WithMessage("*negative*");
    }

    [Fact]
    public void Create_InvalidCurrency_ThrowsDomainException()
    {
        var act = () => Money.Create(10m, "US");
        act.Should().Throw<OrderDomainException>().WithMessage("*3-letter*");
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsDomainException()
    {
        var usd = Money.Create(10m, "USD");
        var eur = Money.Create(10m, "EUR");
        var act = () => usd.Add(eur);
        act.Should().Throw<OrderDomainException>().WithMessage("*Cannot add*");
    }

    [Fact]
    public void Multiply_ReturnsProduct()
    {
        var m = Money.Create(15m);
        m.Multiply(3).Amount.Should().Be(45m);
    }
}
