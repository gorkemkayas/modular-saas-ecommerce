using Notification.Domain.Entities;
using Notification.Domain.Enums;

namespace Notification.Domain.Repositories;

public interface INotificationTemplateRepository
{
    Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken = default);
    Task<NotificationTemplate?> GetByIdAsync(Guid storeId, Guid templateId, CancellationToken cancellationToken = default);
    Task<NotificationTemplate?> GetActiveAsync(
        Guid storeId,
        NotificationTrigger trigger,
        NotificationChannel channel,
        string locale,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsByKeyAsync(
        Guid storeId,
        NotificationTrigger trigger,
        NotificationChannel channel,
        string locale,
        CancellationToken cancellationToken = default);
}
