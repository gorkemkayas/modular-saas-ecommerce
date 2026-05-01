using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Payment.Domain.Entities.Payment payment, CancellationToken cancellationToken = default)
    {
        return _context.Payments.AddAsync(payment, cancellationToken).AsTask();
    }

    public Task<Payment.Domain.Entities.Payment?> GetByIdAsync(Guid storeId, Guid paymentId, CancellationToken cancellationToken = default)
    {
        return _context.Payments
            .Include(x => x.Attempts)
            .Include(x => x.Refunds)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == paymentId, cancellationToken);
    }

    public Task<Payment.Domain.Entities.Payment?> GetByOrderIdAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default)
    {
        return _context.Payments
            .Include(x => x.Attempts)
            .Include(x => x.Refunds)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.OrderId == orderId, cancellationToken);
    }

    public Task<Payment.Domain.Entities.Payment?> GetByProviderReferenceAsync(
        PaymentProvider provider,
        string? externalConversationId,
        string? externalPaymentReference,
        CancellationToken cancellationToken = default)
    {
        return _context.Payments
            .Include(x => x.Attempts)
            .Include(x => x.Refunds)
            .FirstOrDefaultAsync(
                x => x.Provider == provider
                    && ((!string.IsNullOrWhiteSpace(externalConversationId) && x.ExternalConversationId == externalConversationId)
                        || (!string.IsNullOrWhiteSpace(externalPaymentReference) && x.ExternalPaymentReference == externalPaymentReference)),
                cancellationToken);
    }
}
