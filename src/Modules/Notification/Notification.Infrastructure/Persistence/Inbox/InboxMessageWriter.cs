using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Notification.Infrastructure.Persistence.Inbox;

public sealed class InboxMessageWriter : IInboxMessageWriter
{
    private const string PrimaryKeyConstraintName =
        "PK_InboxMessages";

    private readonly NotificationDbContext _dbContext;

    public InboxMessageWriter(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InboxMessageWriteResult> WriteAsync(
        InboxMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        _dbContext.InboxMessages.Add(message);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return InboxMessageWriteResult.Inserted;
        }
        catch (DbUpdateException exception)
            when (IsDuplicateMessage(exception))
        {
            _dbContext.Entry(message).State =
                EntityState.Detached;

            return InboxMessageWriteResult.Duplicate;
        }
    }

    private static bool IsDuplicateMessage(
        DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: PrimaryKeyConstraintName
        };
    }
}