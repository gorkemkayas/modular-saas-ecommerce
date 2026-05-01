using Notification.Domain.Enums;

namespace ECommerce.API.Contracts.Notification;

public sealed record CreateNotificationTemplateRequest(
    NotificationTrigger Trigger,
    NotificationChannel Channel,
    string Locale,
    string Name,
    string SubjectTemplate,
    string BodyTemplate);

public sealed record UpdateNotificationTemplateRequest(
    string Locale,
    string Name,
    string SubjectTemplate,
    string BodyTemplate);

public sealed record SearchNotificationTemplatesRequest(
    NotificationTrigger? Trigger,
    NotificationChannel? Channel,
    bool? IsActive);

public sealed record SearchNotificationDispatchesRequest(
    NotificationTrigger? Trigger,
    NotificationChannel? Channel,
    NotificationStatus? Status,
    string? BusinessEntityType,
    Guid? BusinessEntityId,
    int PageNumber = 1,
    int PageSize = 20);
