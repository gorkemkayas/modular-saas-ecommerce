using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.UnitTests.ValueObjects;

[TestClass]
public sealed class CurrencyTests
{
    [TestMethod]
    public void Create_WithLowercaseCode_NormalizesToUppercase()
    {
        var currency = Currency.Create("try");

        Assert.AreEqual("TRY", currency.Code);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("  ")]
    [DataRow("TR")]
    [DataRow("TRY1")]
    [DataRow("12A")]
    public void Create_WithInvalidCode_ThrowsArgumentException(string code)
    {
        Assert.ThrowsExactly<ArgumentException>(() => Currency.Create(code));
    }

    [TestMethod]
    public void Equals_WithSameCode_ReturnsTrue()
    {
        Assert.AreEqual(Currency.Create("TRY"), Currency.Create("try"));
    }
}
