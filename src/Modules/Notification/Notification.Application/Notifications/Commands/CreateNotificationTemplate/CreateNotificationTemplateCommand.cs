using MediatR;
using Notification.Domain.Enums;

namespace Notification.Application.Notifications.Commands.CreateNotificationTemplate;

public sealed record CreateNotificationTemplateCommand(
    Guid StoreId,
    NotificationTrigger Trigger,
    NotificationChannel Channel,
    string Locale,
    string Name,
    string SubjectTemplate,
    string BodyTemplate) : IRequest<Guid>;
