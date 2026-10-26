namespace Notification.Infrastructure.Persistence.Dispatch;

public sealed record ClaimedNotificationDispatch(
    Guid Id,
    Guid LockId,
    int AttemptCount,
    string? Payload,
    string? ProviderName,
    DateTime? PreparedAtUtc);
