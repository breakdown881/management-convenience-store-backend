using ConvenienceStore.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace ConvenienceStore.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void Addition_WithSameCurrency_ShouldAddAmounts()
    {
        var item1 = new Money(25000m, "VND");
        var item2 = new Money(15000m, "VND");

        var total = item1 + item2;

        total.Amount.Should().Be(40000m);
        total.Currency.Should().Be("VND");
    }

    [Fact]
    public void Addition_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        var vnd = new Money(25000m, "VND");
        var usd = new Money(1m, "USD");

        var act = () => _ = vnd + usd;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var money1 = new Money(100000m, "VND");
        var money2 = new Money(100000m, "VND");

        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }
}