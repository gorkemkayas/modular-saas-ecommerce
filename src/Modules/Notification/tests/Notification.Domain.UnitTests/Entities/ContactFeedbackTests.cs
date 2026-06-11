using Notification.Domain.Entities;
using Notification.Domain.Exceptions;

namespace Notification.Domain.UnitTests.Entities;

[TestClass]
public sealed class ContactFeedbackTests
{
    [TestMethod]
    public void Create_WithValidValues_CapturesFeedbackDetails()
    {
        var feedback = ContactFeedback.Create(
            "Jane Doe",
            "jane@example.com",
            "Homepage feedback",
            "The platform looks promising.",
            "homepage-feedback");

        Assert.AreEqual("Jane Doe", feedback.FullName);
        Assert.AreEqual("jane@example.com", feedback.Email);
        Assert.AreEqual("Homepage feedback", feedback.Subject);
        Assert.AreEqual("The platform looks promising.", feedback.Message);
        Assert.AreEqual("homepage-feedback", feedback.Source);
        Assert.AreNotEqual(Guid.Empty, feedback.Id);
        Assert.IsTrue(feedback.CreatedAtUtc <= DateTime.UtcNow);
    }

    [TestMethod]
    public void Create_WithInvalidEmail_ThrowsDomainException()
    {
        try
        {
            ContactFeedback.Create(
                "Jane Doe",
                "not-an-email",
                "Homepage feedback",
                "The platform looks promising.",
                "homepage-feedback");

            Assert.Fail("Expected NotificationDomainException to be thrown.");
        }
        catch (NotificationDomainException exception)
        {
            Assert.AreEqual("Email address is not valid.", exception.Message);
        }
    }
}
