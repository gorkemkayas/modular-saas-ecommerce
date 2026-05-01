using MediatR;

namespace Notification.Application.Notifications.Commands.DeactivateNotificationTemplate;

public sealed record DeactivateNotificationTemplateCommand(
    Guid StoreId,
    Guid TemplateId) : IRequest;
