using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Domain.Exceptions;

namespace Notification.Domain.UnitTests.Entities;

[TestClass]
public sealed class NotificationDispatchTests
{
    [TestMethod]
    public void RecordFailedAttempt_ThenMarkSent_PreservesHistoryAndCompletesDispatch()
    {
        var dispatch = NotificationDispatch.Create(
            Guid.NewGuid(), NotificationChannel.Email, NotificationTrigger.OrderCancelled,
            "Order", Guid.NewGuid(), null, "customer@example.com", "Customer");
        dispatch.SetRenderedContent("Subject", "Body");

        dispatch.RecordFailedAttempt("Resend", null, "rate_limit_exceeded", "Try later");

        Assert.AreEqual(NotificationStatus.Pending, dispatch.Status);
        Assert.AreEqual(NotificationAttemptStatus.Failed, dispatch.Attempts.Single().Status);

        dispatch.MarkSent("Resend", "request", "email-id");

        Assert.AreEqual(NotificationStatus.Sent, dispatch.Status);
        Assert.HasCount(2, dispatch.Attempts);
        CollectionAssert.AreEqual(new[] { 1, 2 }, dispatch.Attempts.Select(x => x.AttemptNumber).ToArray());
        Assert.IsNull(dispatch.FailureCode);
        Assert.IsNull(dispatch.FailureMessage);
    }

    [TestMethod]
    public void RecordFailedAttempt_AfterPermanentFailure_RejectsTransition()
    {
        var dispatch = NotificationDispatch.Create(
            Guid.NewGuid(), NotificationChannel.Email, NotificationTrigger.OrderCancelled,
            "Order", Guid.NewGuid(), null, "customer@example.com", "Customer");
        dispatch.MarkFailed("Resend", null, "invalid_recipient", "Rejected");

        Assert.ThrowsExactly<NotificationDomainException>(() =>
            dispatch.RecordFailedAttempt("Resend", null, "retry", "Not allowed"));
    }

    [TestMethod]
    public void MarkSent_AfterRendering_SetsStatusAndCreatesAttempt()
    {
        var dispatch = NotificationDispatch.Create(
            Guid.NewGuid(),
            NotificationChannel.Email,
            NotificationTrigger.OrderPlaced,
            "Order",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            "Jane Doe");

        dispatch.SetRenderedContent("Subject", "Body");
        dispatch.MarkSent("Mock", "request-1", "message-1");

        Assert.AreEqual(NotificationStatus.Sent, dispatch.Status);
        Assert.AreEqual("Mock", dispatch.ProviderName);
        Assert.AreEqual("message-1", dispatch.ProviderMessageId);
        Assert.HasCount(1, dispatch.Attempts);
        Assert.IsNotNull(dispatch.SentAtUtc);
    }

    [TestMethod]
    public void MarkSuppressed_SetsStatusAndReason()
    {
        var dispatch = NotificationDispatch.Create(
            Guid.NewGuid(),
            NotificationChannel.Email,
            NotificationTrigger.PaymentFailed,
            "Payment",
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null);

        dispatch.MarkSuppressed("Recipient email address is missing.");

        Assert.AreEqual(NotificationStatus.Suppressed, dispatch.Status);
        Assert.AreEqual("Recipient email address is missing.", dispatch.SuppressionReason);
    }
}
