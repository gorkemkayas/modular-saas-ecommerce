namespace Notification.Application.Feedbacks.DTOs;

public sealed record ContactFeedbackDto(
    Guid Id,
    string FullName,
    string Subject,
    string Message,
    string? Source,
    DateTime CreatedAtUtc);
