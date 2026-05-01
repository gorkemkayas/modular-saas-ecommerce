using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Domain.Repositories;

namespace Notification.Infrastructure.Persistence.Repositories;

public sealed class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly NotificationDbContext _context;

    public NotificationTemplateRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        return _context.NotificationTemplates.AddAsync(template, cancellationToken).AsTask();
    }

    public Task<NotificationTemplate?> GetByIdAsync(Guid storeId, Guid templateId, CancellationToken cancellationToken = default)
    {
        return _context.NotificationTemplates.FirstOrDefaultAsync(
            x => x.StoreId == storeId && x.Id == templateId,
            cancellationToken);
    }

    public Task<NotificationTemplate?> GetActiveAsync(
        Guid storeId,
        NotificationTrigger trigger,
        NotificationChannel channel,
        string locale,
        CancellationToken cancellationToken = default)
    {
        return _context.NotificationTemplates.FirstOrDefaultAsync(
            x => x.StoreId == storeId
                && x.Trigger == trigger
                && x.Channel == channel
                && x.Locale == locale
                && x.IsActive,
            cancellationToken);
    }

    public Task<bool> ExistsByKeyAsync(
        Guid storeId,
        NotificationTrigger trigger,
        NotificationChannel channel,
        string locale,
        CancellationToken cancellationToken = default)
    {
        return _context.NotificationTemplates.AnyAsync(
            x => x.StoreId == storeId
                && x.Trigger == trigger
                && x.Channel == channel
                && x.Locale == locale,
            cancellationToken);
    }
}
