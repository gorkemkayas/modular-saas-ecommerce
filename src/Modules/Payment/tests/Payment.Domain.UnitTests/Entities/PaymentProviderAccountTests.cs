using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Domain.UnitTests.Entities;

[TestClass]
public sealed class PaymentProviderAccountTests
{
    [TestMethod]
    public void CreateIyzico_WhenEnabledWithCredentials_MarksAccountReady()
    {
        var account = PaymentProviderAccount.CreateIyzico(
            Guid.NewGuid(),
            "protected-api-key",
            "protected-secret-key",
            "1234",
            isEnabled: true);

        Assert.AreEqual(PaymentProvider.Iyzico, account.Provider);
        Assert.AreEqual(PaymentProviderAccountStatus.Active, account.Status);
        Assert.IsTrue(account.IsReadyForPayments);
    }
}
