using Microsoft.EntityFrameworkCore;
using Notification.Application.Abstractions;
using Notification.Domain.Entities;
using Notification.Infrastructure.Persistence.Inbox;

namespace Notification.Infrastructure.Persistence;

public sealed class NotificationDbContext : DbContext, IUnitOfWork
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ContactFeedback> ContactFeedbacks => Set<ContactFeedback>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationDispatch> NotificationDispatches => Set<NotificationDispatch>();
    public DbSet<NotificationAttempt> NotificationAttempts => Set<NotificationAttempt>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
