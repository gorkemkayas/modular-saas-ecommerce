using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Notification.Application.Abstractions;
using Notification.Application.Notifications.Services;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Domain.Repositories;

namespace Notification.Application.UnitTests.Notifications.Services;

[TestClass]
public sealed class NotificationSenderTests
{
    [TestMethod]
    public async Task SendAsync_WhenDispatchAlreadyExists_ReturnsExistingDispatchIdWithoutSending()
    {
        var templateRepository = new Mock<INotificationTemplateRepository>();
        var dispatchRepository = new Mock<INotificationDispatchRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var templateRenderer = new Mock<ITemplateRenderer>();
        var emailGateway = new Mock<IEmailGateway>();

        var existingDispatch = NotificationDispatch.Create(
            Guid.NewGuid(),
            NotificationChannel.Email,
            NotificationTrigger.OrderPlaced,
            "Order",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            "Jane Doe");

        dispatchRepository
            .Setup(x => x.GetByBusinessKeyAsync(
                existingDispatch.StoreId,
                NotificationChannel.Email,
                NotificationTrigger.OrderPlaced,
                "Order",
                existingDispatch.BusinessEntityId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDispatch);

        var sender = new NotificationSender(
            templateRepository.Object,
            dispatchRepository.Object,
            unitOfWork.Object,
            templateRenderer.Object,
            emailGateway.Object,
            NullLogger<NotificationSender>.Instance);

        var dispatchId = await sender.SendAsync(
            new TransactionalNotificationRequest(
                existingDispatch.StoreId,
                NotificationChannel.Email,
                NotificationTrigger.OrderPlaced,
                "default",
                "Order",
                existingDispatch.BusinessEntityId,
                Guid.NewGuid(),
                "customer@example.com",
                "Jane Doe",
                new Dictionary<string, string?>()),
            CancellationToken.None);

        Assert.AreEqual(existingDispatch.Id, dispatchId);
        dispatchRepository.Verify(x => x.AddAsync(It.IsAny<NotificationDispatch>(), It.IsAny<CancellationToken>()), Times.Never);
        templateRepository.Verify(x => x.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<NotificationTrigger>(), It.IsAny<NotificationChannel>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        emailGateway.Verify(x => x.SendAsync(It.IsAny<EmailSendRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task SendAsync_WhenTemplateIsMissing_MarksDispatchSuppressed()
    {
        var templateRepository = new Mock<INotificationTemplateRepository>();
        var dispatchRepository = new Mock<INotificationDispatchRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var templateRenderer = new Mock<ITemplateRenderer>();
        var emailGateway = new Mock<IEmailGateway>();

        NotificationDispatch? addedDispatch = null;

        dispatchRepository
            .Setup(x => x.GetByBusinessKeyAsync(
                It.IsAny<Guid>(),
                It.IsAny<NotificationChannel>(),
                It.IsAny<NotificationTrigger>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationDispatch?)null);

        dispatchRepository
            .Setup(x => x.AddAsync(It.IsAny<NotificationDispatch>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationDispatch, CancellationToken>((dispatch, _) => addedDispatch = dispatch)
            .Returns(Task.CompletedTask);

        templateRepository
            .Setup(x => x.GetActiveAsync(
                It.IsAny<Guid>(),
                NotificationTrigger.PaymentCaptured,
                NotificationChannel.Email,
                "default",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationTemplate?)null);

        var sender = new NotificationSender(
            templateRepository.Object,
            dispatchRepository.Object,
            unitOfWork.Object,
            templateRenderer.Object,
            emailGateway.Object,
            NullLogger<NotificationSender>.Instance);

        var dispatchId = await sender.SendAsync(
            new TransactionalNotificationRequest(
                Guid.NewGuid(),
                NotificationChannel.Email,
                NotificationTrigger.PaymentCaptured,
                "default",
                "Payment",
                Guid.NewGuid(),
                Guid.NewGuid(),
                "customer@example.com",
                "Jane Doe",
                new Dictionary<string, string?>()),
            CancellationToken.None);

        Assert.IsNotNull(addedDispatch);
        Assert.AreEqual(dispatchId, addedDispatch.Id);
        Assert.AreEqual(NotificationStatus.Suppressed, addedDispatch.Status);
        Assert.AreEqual("Active notification template could not be resolved.", addedDispatch.SuppressionReason);
        emailGateway.Verify(x => x.SendAsync(It.IsAny<EmailSendRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
