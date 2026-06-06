using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Domain.Exceptions;

namespace Payment.Domain.UnitTests.Entities;

[TestClass]
public sealed class PaymentTests
{
    [TestMethod]
    public void Create_WithValidArguments_CreatesPendingPayment()
    {
        var payment = Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1001",
            Guid.NewGuid(),
            250m,
            "TRY",
            PaymentProvider.Mock,
            PaymentMethodType.Card);

        Assert.AreNotEqual(Guid.Empty, payment.Id);
        Assert.AreEqual(PaymentStatus.Pending, payment.Status);
        Assert.AreEqual(250m, payment.Amount);
        Assert.AreEqual("TRY", payment.CurrencyCode);
        Assert.AreEqual(PaymentProvider.Mock, payment.Provider);
    }

    [TestMethod]
    public void Refund_WhenRefundsRemainingAmount_MarksPaymentRefunded()
    {
        var payment = CreateCapturedPayment();

        payment.Refund("refund-1", 100m, "Customer requested refund.", "refund-ref-1");

        Assert.AreEqual(PaymentStatus.Refunded, payment.Status);
        Assert.AreEqual(100m, payment.RefundedAmount);
        Assert.AreEqual(1, payment.Refunds.Count);
    }

    [TestMethod]
    public void Refund_WhenAmountExceedsRemaining_ThrowsPaymentDomainException()
    {
        var payment = CreateCapturedPayment();

        Assert.ThrowsExactly<PaymentDomainException>(() =>
            payment.Refund("refund-1", 120m, "Customer requested refund.", "refund-ref-1"));
    }

    [TestMethod]
    public void AssignProviderAccount_WhenDifferentAccountAlreadyAssigned_ThrowsPaymentDomainException()
    {
        var payment = Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1003",
            Guid.NewGuid(),
            100m,
            "TRY",
            PaymentProvider.Iyzico,
            PaymentMethodType.Card);

        payment.AssignProviderAccount(Guid.NewGuid());

        Assert.ThrowsExactly<PaymentDomainException>(() =>
            payment.AssignProviderAccount(Guid.NewGuid()));
    }

    private static Payment.Domain.Entities.Payment CreateCapturedPayment()
    {
        var payment = Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1002",
            Guid.NewGuid(),
            100m,
            "TRY",
            PaymentProvider.Mock,
            PaymentMethodType.Card);

        payment.MarkCaptured(
            "capture-1",
            "conv-1",
            "pay-ref-1");

        return payment;
    }
}
