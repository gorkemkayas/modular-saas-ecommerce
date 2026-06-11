using Notification.Domain.Entities;
using Notification.Domain.Repositories;

namespace Notification.Infrastructure.Persistence.Repositories;

public sealed class ContactFeedbackRepository : IContactFeedbackRepository
{
    private readonly NotificationDbContext _context;

    public ContactFeedbackRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(ContactFeedback feedback, CancellationToken cancellationToken = default)
    {
        return _context.ContactFeedbacks.AddAsync(feedback, cancellationToken).AsTask();
    }
}
