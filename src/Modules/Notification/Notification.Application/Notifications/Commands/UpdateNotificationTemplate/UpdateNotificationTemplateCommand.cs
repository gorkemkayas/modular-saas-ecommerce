using MediatR;

namespace Notification.Application.Notifications.Commands.UpdateNotificationTemplate;

public sealed record UpdateNotificationTemplateCommand(
    Guid StoreId,
    Guid TemplateId,
    string Locale,
    string Name,
    string SubjectTemplate,
    string BodyTemplate) : IRequest;
