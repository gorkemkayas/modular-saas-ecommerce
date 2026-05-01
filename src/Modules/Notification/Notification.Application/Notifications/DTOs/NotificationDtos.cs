using Notification.Domain.Enums;

namespace Notification.Application.Notifications.DTOs;

public sealed record NotificationTemplateSummaryDto(
    Guid Id,
    NotificationTrigger Trigger,
    NotificationChannel Channel,
    string Locale,
    string Name,
    bool IsActive,
    DateTime UpdatedAtUtc);

public sealed record NotificationTemplateDto(
    Guid Id,
    Guid StoreId,
    NotificationTrigger Trigger,
    NotificationChannel Channel,
    string Locale,
    string Name,
    string SubjectTemplate,
    string BodyTemplate,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record NotificationDispatchSummaryDto(
    Guid Id,
    NotificationChannel Channel,
    NotificationTrigger Trigger,
    NotificationStatus Status,
    string? RecipientAddress,
    string BusinessEntityType,
    Guid BusinessEntityId,
    string? ProviderName,
    string? LastProviderEventType,
    DateTime CreatedAtUtc,
    DateTime? SentAtUtc);

public sealed record NotificationDispatchDto(
    Guid Id,
    Guid StoreId,
    NotificationChannel Channel,
    NotificationTrigger Trigger,
    NotificationStatus Status,
    string? RecipientAddress,
    string? RecipientName,
    string? Subject,
    string? Body,
    string BusinessEntityType,
    Guid BusinessEntityId,
    Guid? CustomerId,
    string? ProviderName,
    string? ProviderMessageId,
    string? FailureCode,
    string? FailureMessage,
    string? SuppressionReason,
    string? LastProviderEventType,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? SentAtUtc,
    DateTime? LastAttemptAtUtc,
    DateTime? LastProviderEventAtUtc,
    DateTime? DeliveredAtUtc,
    DateTime? OpenedAtUtc,
    DateTime? ClickedAtUtc,
    DateTime? BouncedAtUtc,
    DateTime? ComplainedAtUtc,
    IReadOnlyCollection<NotificationAttemptDto> Attempts);

public sealed record NotificationAttemptDto(
    Guid Id,
    int AttemptNumber,
    NotificationAttemptStatus Status,
    string ProviderName,
    string? ProviderRequestReference,
    string? ProviderMessageId,
    string? FailureCode,
    string? FailureMessage,
    DateTime AttemptedAtUtc);

public sealed record NotificationDispatchSearchCriteria(
    Guid StoreId,
    NotificationTrigger? Trigger,
    NotificationChannel? Channel,
    NotificationStatus? Status,
    string? BusinessEntityType,
    Guid? BusinessEntityId,
    int PageNumber,
    int PageSize);
