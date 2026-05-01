using MediatR;
using Notification.Application.Abstractions.Queries;
using Notification.Application.Notifications.DTOs;

namespace Notification.Application.Notifications.Queries.GetNotificationDispatchById;

public sealed class GetNotificationDispatchByIdQueryHandler : IRequestHandler<GetNotificationDispatchByIdQuery, NotificationDispatchDto?>
{
    private readonly INotificationReadService _readService;

    public GetNotificationDispatchByIdQueryHandler(INotificationReadService readService)
    {
        _readService = readService;
    }

    public Task<NotificationDispatchDto?> Handle(GetNotificationDispatchByIdQuery query, CancellationToken cancellationToken)
    {
        return _readService.GetDispatchByIdAsync(query.StoreId, query.DispatchId, cancellationToken);
    }
}
