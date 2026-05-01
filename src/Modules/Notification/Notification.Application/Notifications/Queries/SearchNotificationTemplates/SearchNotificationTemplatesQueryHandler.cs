using MediatR;
using Notification.Application.Abstractions.Queries;
using Notification.Application.Notifications.DTOs;

namespace Notification.Application.Notifications.Queries.SearchNotificationTemplates;

public sealed class SearchNotificationTemplatesQueryHandler : IRequestHandler<SearchNotificationTemplatesQuery, IReadOnlyCollection<NotificationTemplateSummaryDto>>
{
    private readonly INotificationReadService _readService;

    public SearchNotificationTemplatesQueryHandler(INotificationReadService readService)
    {
        _readService = readService;
    }

    public Task<IReadOnlyCollection<NotificationTemplateSummaryDto>> Handle(SearchNotificationTemplatesQuery query, CancellationToken cancellationToken)
    {
        return _readService.SearchTemplatesAsync(
            query.StoreId,
            query.Trigger,
            query.Channel,
            query.IsActive,
            cancellationToken);
    }
}
