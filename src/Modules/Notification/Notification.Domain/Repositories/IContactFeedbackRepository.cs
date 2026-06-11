using Notification.Domain.Entities;

namespace Notification.Domain.Repositories;

public interface IContactFeedbackRepository
{
    Task AddAsync(ContactFeedback feedback, CancellationToken cancellationToken = default);
}
