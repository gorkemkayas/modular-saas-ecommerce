using MediatR;
using Notification.Application.Common.Models;
using Notification.Application.Notifications.DTOs;
using Notification.Domain.Enums;

namespace Notification.Application.Notifications.Queries.SearchNotificationDispatches;

public sealed record SearchNotificationDispatchesQuery(
    Guid StoreId,
    NotificationTrigger? Trigger,
    NotificationChannel? Channel,
    NotificationStatus? Status,
    string? BusinessEntityType,
    Guid? BusinessEntityId,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<NotificationDispatchSummaryDto>>;
