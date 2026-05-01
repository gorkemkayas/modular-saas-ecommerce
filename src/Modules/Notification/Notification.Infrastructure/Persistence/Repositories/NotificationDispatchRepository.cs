using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Domain.Repositories;

namespace Notification.Infrastructure.Persistence.Repositories;

public sealed class NotificationDispatchRepository : INotificationDispatchRepository
{
    private readonly NotificationDbContext _context;

    public NotificationDispatchRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(NotificationDispatch dispatch, CancellationToken cancellationToken = default)
    {
        return _context.NotificationDispatches.AddAsync(dispatch, cancellationToken).AsTask();
    }

    public Task<NotificationDispatch?> GetByIdAsync(Guid storeId, Guid dispatchId, CancellationToken cancellationToken = default)
    {
        return _context.NotificationDispatches
            .Include(x => x.Attempts)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == dispatchId, cancellationToken);
    }

    public Task<NotificationDispatch?> GetByProviderMessageIdAsync(
        string providerName,
        string providerMessageId,
        CancellationToken cancellationToken = default)
    {
        return _context.NotificationDispatches
            .Include(x => x.Attempts)
            .FirstOrDefaultAsync(
                x => x.ProviderName == providerName && x.ProviderMessageId == providerMessageId,
                cancellationToken);
    }

    public Task<NotificationDispatch?> GetByBusinessKeyAsync(
        Guid storeId,
        NotificationChannel channel,
        NotificationTrigger trigger,
        string businessEntityType,
        Guid businessEntityId,
        CancellationToken cancellationToken = default)
    {
        return _context.NotificationDispatches
            .Include(x => x.Attempts)
            .FirstOrDefaultAsync(
                x => x.StoreId == storeId
                    && x.Channel == channel
                    && x.Trigger == trigger
                    && x.BusinessEntityType == businessEntityType
                    && x.BusinessEntityId == businessEntityId,
                cancellationToken);
    }
}
