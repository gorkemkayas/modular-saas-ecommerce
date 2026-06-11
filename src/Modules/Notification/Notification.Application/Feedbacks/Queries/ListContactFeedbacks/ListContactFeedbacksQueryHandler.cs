using MediatR;
using Notification.Application.Abstractions.Queries;
using Notification.Application.Feedbacks.DTOs;

namespace Notification.Application.Feedbacks.Queries.ListContactFeedbacks;

public sealed class ListContactFeedbacksQueryHandler
    : IRequestHandler<ListContactFeedbacksQuery, IReadOnlyCollection<ContactFeedbackDto>>
{
    private readonly INotificationReadService _readService;

    public ListContactFeedbacksQueryHandler(INotificationReadService readService)
    {
        _readService = readService;
    }

    public Task<IReadOnlyCollection<ContactFeedbackDto>> Handle(
        ListContactFeedbacksQuery query,
        CancellationToken cancellationToken)
    {
        return _readService.ListContactFeedbacksAsync(cancellationToken);
    }
}
