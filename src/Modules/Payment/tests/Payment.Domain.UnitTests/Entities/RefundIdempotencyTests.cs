using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Domain.Exceptions;
using Payment.Domain.Events;
using PaymentEntity = Payment.Domain.Entities.Payment;

namespace Payment.Domain.UnitTests.Entities;

[TestClass]
public sealed class RefundIdempotencyTests
{
    [TestMethod]
    [DataRow(20)]
    [DataRow(100)]
    public void Replay_PreservesRefundIdentityAndEmitsNoSecondEvent(int amount)
    {
        var payment = Captured();
        payment.Refund("refund", amount, "Customer request", "provider-payment-reference");
        Guid id = payment.Refunds.Single().Id;
        payment.ClearDomainEvents();

        payment.Refund("refund", amount, "Payment refunded via webhook.", "provider-payment-reference");

        Assert.HasCount(1, payment.Refunds);
        Assert.AreEqual(id, payment.Refunds.Single().Id);
        Assert.AreEqual((decimal)amount, payment.RefundedAmount);
        Assert.HasCount(0, payment.DomainEvents);
        Assert.HasCount(1, payment.Attempts.Where(x => x.OperationType == PaymentOperationType.Refund));
    }

    [TestMethod]
    public void SameKeyWithDifferentRequest_IsRejected()
    {
        var payment = Captured();
        payment.Refund("refund", 20m, "Original reason", "ref");
        Assert.ThrowsExactly<PaymentDomainException>(() => payment.ValidateRefundRequest("refund", 30m, "Original reason"));
        Assert.ThrowsExactly<PaymentDomainException>(() => payment.ValidateRefundRequest("refund", 20m, "Other reason"));
        Assert.ThrowsExactly<PaymentDomainException>(() => payment.Refund("refund", 30m, "Original reason", "ref"));
        Assert.AreEqual(20m, payment.RefundedAmount);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void RefundFailure_PreservesCapturedStateAndDoesNotEmitPaymentFailure(bool partial)
    {
        var payment = Captured();
        if (partial) payment.Refund("earlier", 10m, "Earlier refund", "ref");
        payment.ClearDomainEvents();
        var status = payment.Status;

        payment.MarkFailed("rejected", PaymentOperationType.Refund, "rejected", "Refund rejected");

        Assert.AreEqual(status, payment.Status);
        Assert.IsNull(payment.FailedAtUtc);
        Assert.IsNull(payment.FailureCode);
        Assert.HasCount(0, payment.DomainEvents);
        Assert.AreEqual(PaymentAttemptStatus.Failed, payment.Attempts.Last().Status);
        payment.Refund("rejected", 20m, "Retry succeeded", "ref");
        Assert.HasCount(2, payment.Attempts.Where(x => x.IdempotencyKey == "rejected"));
        Assert.HasCount(1, payment.DomainEvents.OfType<PaymentStatusChangedDomainEvent>());
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(20)]
    [DataRow(100)]
    public void UnclassifiedFailureCallback_CannotDowngradePaidOrRefundedPayment(int refunded)
    {
        var payment = Captured();
        if (refunded > 0) payment.Refund("refund", refunded, "Reason", "ref");
        payment.ClearDomainEvents();
        var status = payment.Status;
        Assert.ThrowsExactly<PaymentDomainException>(() => payment.MarkFailed("callback", PaymentOperationType.Webhook,
            "declined", "Unclassified failure"));
        Assert.AreEqual(status, payment.Status);
        Assert.HasCount(0, payment.DomainEvents);
    }

    [TestMethod]
    public void LegacyRefundsWithReusedProviderReference_RequireReconciliation()
    {
        var payment = Captured();
        payment.Refund("first", 10m, "First", "same-provider-payment-ref");
        payment.Refund("second", 10m, "Second", "same-provider-payment-ref");
        // Simulate pre-migration rows whose only operation identifiers live in attempts.
        foreach (PaymentRefund refund in payment.Refunds)
            typeof(PaymentRefund).GetProperty(nameof(PaymentRefund.IdempotencyKey))!.SetValue(refund, null);

        Assert.ThrowsExactly<PaymentDomainException>(() => payment.GetRefundByIdempotencyKey("first"));
        Assert.HasCount(2, payment.Refunds);
    }

    private static PaymentEntity Captured()
    {
        var payment = PaymentEntity.Create(Guid.NewGuid(), Guid.NewGuid(), "ORD-REFUND", Guid.NewGuid(),
            100m, "TRY", PaymentProvider.Mock, PaymentMethodType.Card);
        payment.MarkCaptured("capture", null, "ref");
        payment.ClearDomainEvents();
        return payment;
    }
}
