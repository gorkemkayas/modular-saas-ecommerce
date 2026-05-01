using MediatR;

namespace Notification.Application.Notifications.Commands.ActivateNotificationTemplate;

public sealed record ActivateNotificationTemplateCommand(
    Guid StoreId,
    Guid TemplateId) : IRequest;
