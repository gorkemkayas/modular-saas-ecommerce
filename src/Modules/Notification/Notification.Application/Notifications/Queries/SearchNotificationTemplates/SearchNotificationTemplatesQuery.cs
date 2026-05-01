using MediatR;
using Notification.Application.Notifications.DTOs;
using Notification.Domain.Enums;

namespace Notification.Application.Notifications.Queries.SearchNotificationTemplates;

public sealed record SearchNotificationTemplatesQuery(
    Guid StoreId,
    NotificationTrigger? Trigger,
    NotificationChannel? Channel,
    bool? IsActive) : IRequest<IReadOnlyCollection<NotificationTemplateSummaryDto>>;
