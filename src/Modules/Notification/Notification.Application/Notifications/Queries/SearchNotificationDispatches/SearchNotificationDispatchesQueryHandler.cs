using MediatR;
using Notification.Application.Abstractions.Queries;
using Notification.Application.Common.Models;
using Notification.Application.Notifications.DTOs;

namespace Notification.Application.Notifications.Queries.SearchNotificationDispatches;

public sealed class SearchNotificationDispatchesQueryHandler
    : IRequestHandler<SearchNotificationDispatchesQuery, PagedResult<NotificationDispatchSummaryDto>>
{
    private readonly INotificationReadService _readService;

    public SearchNotificationDispatchesQueryHandler(INotificationReadService readService)
    {
        _readService = readService;
    }

    public Task<PagedResult<NotificationDispatchSummaryDto>> Handle(
        SearchNotificationDispatchesQuery query,
        CancellationToken cancellationToken)
    {
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);

        return _readService.SearchDispatchesAsync(
            new NotificationDispatchSearchCriteria(
                query.StoreId,
                query.Trigger,
                query.Channel,
                query.Status,
                query.BusinessEntityType,
                query.BusinessEntityId,
                pageNumber,
                pageSize),
            cancellationToken);
    }
}
