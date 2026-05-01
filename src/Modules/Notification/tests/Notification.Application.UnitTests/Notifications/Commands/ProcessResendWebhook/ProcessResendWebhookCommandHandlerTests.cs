using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Notification.Application.Abstractions;
using Notification.Application.Exceptions;
using Notification.Application.Notifications.Commands.ProcessResendWebhook;
using Notification.Application.Notifications.Services;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Domain.Repositories;

namespace Notification.Application.UnitTests.Notifications.Commands.ProcessResendWebhook;

[TestClass]
public sealed class ProcessResendWebhookCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WhenDeliveredWebhookIsReceived_UpdatesDispatch()
    {
        var dispatchRepository = new Mock<INotificationDispatchRepository>();
        var webhookVerifier = new Mock<IResendWebhookVerifier>();
        var unitOfWork = new Mock<IUnitOfWork>();

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
        dispatch.MarkSent("Resend", "request-1", "email-1");

        webhookVerifier.SetupGet(x => x.IsVerificationEnabled).Returns(false);

        dispatchRepository
            .Setup(x => x.GetByProviderMessageIdAsync("Resend", "email-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dispatch);

        var handler = new ProcessResendWebhookCommandHandler(
            dispatchRepository.Object,
            webhookVerifier.Object,
            unitOfWork.Object,
            NullLogger<ProcessResendWebhookCommandHandler>.Instance);

        await handler.Handle(
            new ProcessResendWebhookCommand(
                """
                {
                  "type": "email.delivered",
                  "created_at": "2026-05-01T10:00:00Z",
                  "data": {
                    "email_id": "email-1",
                    "created_at": "2026-05-01T09:59:58Z"
                  }
                }
                """,
                "msg_1",
                "1714557600",
                "v1,signature"),
            CancellationToken.None);

        Assert.AreEqual(NotificationStatus.Sent, dispatch.Status);
        Assert.AreEqual("email.delivered", dispatch.LastProviderEventType);
        Assert.AreEqual(new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), dispatch.DeliveredAtUtc);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenVerificationFails_ThrowsValidationException()
    {
        var dispatchRepository = new Mock<INotificationDispatchRepository>();
        var webhookVerifier = new Mock<IResendWebhookVerifier>();
        var unitOfWork = new Mock<IUnitOfWork>();

        webhookVerifier.SetupGet(x => x.IsVerificationEnabled).Returns(true);
        webhookVerifier
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(false);

        var handler = new ProcessResendWebhookCommandHandler(
            dispatchRepository.Object,
            webhookVerifier.Object,
            unitOfWork.Object,
            NullLogger<ProcessResendWebhookCommandHandler>.Instance);

        try
        {
            await handler.Handle(
                new ProcessResendWebhookCommand("{\"type\":\"email.delivered\",\"data\":{\"email_id\":\"email-1\"}}", "msg_1", "1714557600", "v1,signature"),
                CancellationToken.None);

            Assert.Fail("Expected NotificationWebhookValidationException to be thrown.");
        }
        catch (NotificationWebhookValidationException)
        {
        }

        dispatchRepository.Verify(x => x.GetByProviderMessageIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
