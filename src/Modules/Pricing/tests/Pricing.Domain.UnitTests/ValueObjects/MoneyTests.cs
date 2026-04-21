using Pricing.Domain.Exceptions;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.UnitTests.ValueObjects;

[TestClass]
public sealed class MoneyTests
{
    [TestMethod]
    public void Create_WithValidAmount_CreatesMoney()
    {
        var money = Money.Create(10.125m, "TRY");

        Assert.AreEqual(10.13m, money.Amount);
        Assert.AreEqual("TRY", money.Currency.Code);
    }

    [TestMethod]
    public void Create_WithNegativeAmount_ThrowsPricingDomainException()
    {
        Assert.ThrowsExactly<PricingDomainException>(() => Money.Create(-1m, "TRY"));
    }

    [TestMethod]
    public void Add_WithSameCurrency_ReturnsSum()
    {
        var first = Money.Create(10m, "TRY");
        var second = Money.Create(15.5m, "TRY");

        var result = first.Add(second);

        Assert.AreEqual(25.5m, result.Amount);
        Assert.AreEqual("TRY", result.Currency.Code);
    }

    [TestMethod]
    public void Add_WithDifferentCurrency_ThrowsPricingDomainException()
    {
        var first = Money.Create(10m, "TRY");
        var second = Money.Create(15m, "USD");

        Assert.ThrowsExactly<PricingDomainException>(() => first.Add(second));
    }

    [TestMethod]
    public void Subtract_WhenResultWouldBeNegative_ThrowsPricingDomainException()
    {
        var first = Money.Create(10m, "TRY");
        var second = Money.Create(15m, "TRY");

        Assert.ThrowsExactly<PricingDomainException>(() => first.Subtract(second));
    }
}
