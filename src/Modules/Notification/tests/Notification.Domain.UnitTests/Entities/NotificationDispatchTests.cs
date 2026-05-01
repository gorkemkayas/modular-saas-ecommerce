using Notification.Domain.Entities;
using Notification.Domain.Enums;

namespace Notification.Domain.UnitTests.Entities;

[TestClass]
public sealed class NotificationDispatchTests
{
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
        Assert.AreEqual(1, dispatch.Attempts.Count);
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
