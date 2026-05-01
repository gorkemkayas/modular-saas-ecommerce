using MediatR;
using Notification.Application.Notifications.DTOs;

namespace Notification.Application.Notifications.Queries.GetNotificationTemplateById;

public sealed record GetNotificationTemplateByIdQuery(
    Guid StoreId,
    Guid TemplateId) : IRequest<NotificationTemplateDto?>;
