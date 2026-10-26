using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Domain.Exceptions;

namespace Notification.Domain.UnitTests.Entities;

[TestClass]
public sealed class NotificationTemplateTests
{
    [TestMethod]
    public void CreateDefault_CreatesActiveGlobalTemplate()
    {
        var template = NotificationTemplate.CreateDefault(
            "Default cancellation", NotificationTrigger.OrderCancelled,
            NotificationChannel.Email, "default", "Order cancelled", "Body");

        Assert.AreEqual(Guid.Empty, template.StoreId);
        Assert.AreNotEqual(Guid.Empty, template.Id);
        Assert.IsTrue(template.IsActive);
    }

    [TestMethod]
    public void Create_WithoutStoreId_StillRejectsStoreTemplate()
    {
        Assert.ThrowsExactly<NotificationDomainException>(() =>
            NotificationTemplate.Create(
                Guid.Empty, "Cancellation", NotificationTrigger.OrderCancelled,
                NotificationChannel.Email, "default", "Order cancelled", "Body"));
    }
}
