using MediatR;
using Notification.Application.Abstractions.Queries;
using Notification.Application.Notifications.DTOs;

namespace Notification.Application.Notifications.Queries.GetNotificationTemplateById;

public sealed class GetNotificationTemplateByIdQueryHandler : IRequestHandler<GetNotificationTemplateByIdQuery, NotificationTemplateDto?>
{
    private readonly INotificationReadService _readService;

    public GetNotificationTemplateByIdQueryHandler(INotificationReadService readService)
    {
        _readService = readService;
    }

    public Task<NotificationTemplateDto?> Handle(GetNotificationTemplateByIdQuery query, CancellationToken cancellationToken)
    {
        return _readService.GetTemplateByIdAsync(query.StoreId, query.TemplateId, cancellationToken);
    }
}
