using MediatR;
using Notification.Application.Feedbacks.DTOs;

namespace Notification.Application.Feedbacks.Queries.ListContactFeedbacks;

public sealed record ListContactFeedbacksQuery() : IRequest<IReadOnlyCollection<ContactFeedbackDto>>;
