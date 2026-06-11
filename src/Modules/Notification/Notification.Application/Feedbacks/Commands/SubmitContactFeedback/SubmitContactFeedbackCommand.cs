using MediatR;

namespace Notification.Application.Feedbacks.Commands.SubmitContactFeedback;

public sealed record SubmitContactFeedbackCommand(
    string FullName,
    string Email,
    string Subject,
    string Message,
    string? Source) : IRequest<Guid>;
