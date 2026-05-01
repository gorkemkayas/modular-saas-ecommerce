using MediatR;
using Notification.Application.Notifications.DTOs;

namespace Notification.Application.Notifications.Queries.GetNotificationDispatchById;

public sealed record GetNotificationDispatchByIdQuery(
    Guid StoreId,
    Guid DispatchId) : IRequest<NotificationDispatchDto?>;
