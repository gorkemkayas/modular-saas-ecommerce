using BuildingBlocks.Domain.Events;
using BuildingBlocks.Infrastructure.Messaging.Outbox;
using Payment.Domain.Events;
using Payment.Contracts.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Abstractions;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Persistence;

public sealed class PaymentDbContext : DomainEventOutboxDbContext, IUnitOfWork
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment.Domain.Entities.Payment> Payments => Set<Payment.Domain.Entities.Payment>();
    public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();
    public DbSet<PaymentRefund> PaymentRefunds => Set<PaymentRefund>();
    public DbSet<PaymentProviderAccount> PaymentProviderAccounts => Set<PaymentProviderAccount>();

    protected override Task StageEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (domainEvent is not PaymentStatusChangedDomainEvent e)
            throw new InvalidOperationException($"Unsupported Payment event {domainEvent.GetType().Name}.");
        StageIntegrationEvent(e.StoreId, new PaymentStatusChangedIntegrationEvent(
            e.Id, e.OccurredOnUtc, e.PaymentId, e.StoreId, e.OrderId, e.CustomerId,
            e.OrderNumber, e.Kind switch
            {
                Payment.Domain.Events.PaymentEventKind.Authorized => Payment.Contracts.IntegrationEvents.PaymentEventKind.Authorized,
                Payment.Domain.Events.PaymentEventKind.Captured => Payment.Contracts.IntegrationEvents.PaymentEventKind.Captured,
                Payment.Domain.Events.PaymentEventKind.Failed => Payment.Contracts.IntegrationEvents.PaymentEventKind.Failed,
                Payment.Domain.Events.PaymentEventKind.Refunded => Payment.Contracts.IntegrationEvents.PaymentEventKind.Refunded,
                _ => throw new InvalidOperationException("Unsupported payment event kind.")
            }, e.Amount, e.CurrencyCode, e.PaymentReference, e.Description,
            e.RecipientEmail, e.RecipientName, e.RefundId));
        return Task.CompletedTask;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
