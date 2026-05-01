using Notification.Domain.Entities;
using Notification.Domain.Enums;

namespace Notification.Domain.Repositories;

public interface INotificationDispatchRepository
{
    Task AddAsync(NotificationDispatch dispatch, CancellationToken cancellationToken = default);
    Task<NotificationDispatch?> GetByIdAsync(Guid storeId, Guid dispatchId, CancellationToken cancellationToken = default);
    Task<NotificationDispatch?> GetByProviderMessageIdAsync(
        string providerName,
        string providerMessageId,
        CancellationToken cancellationToken = default);
    Task<NotificationDispatch?> GetByBusinessKeyAsync(
        Guid storeId,
        NotificationChannel channel,
        NotificationTrigger trigger,
        string businessEntityType,
        Guid businessEntityId,
        CancellationToken cancellationToken = default);
}
