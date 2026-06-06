using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Infrastructure.Persistence.Repositories;

public sealed class PaymentProviderAccountRepository : IPaymentProviderAccountRepository
{
    private readonly PaymentDbContext _context;

    public PaymentProviderAccountRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(PaymentProviderAccount account, CancellationToken cancellationToken = default)
    {
        return _context.PaymentProviderAccounts.AddAsync(account, cancellationToken).AsTask();
    }

    public Task<PaymentProviderAccount?> GetByIdAsync(
        Guid storeId,
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        return _context.PaymentProviderAccounts
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == accountId, cancellationToken);
    }

    public Task<PaymentProviderAccount?> GetByStoreAndProviderAsync(
        Guid storeId,
        PaymentProvider provider,
        CancellationToken cancellationToken = default)
    {
        return _context.PaymentProviderAccounts
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Provider == provider, cancellationToken);
    }

    public Task<PaymentProviderAccount?> GetReadyForPaymentsAsync(
        Guid storeId,
        PaymentProvider provider,
        CancellationToken cancellationToken = default)
    {
        return _context.PaymentProviderAccounts
            .FirstOrDefaultAsync(
                x => x.StoreId == storeId
                    && x.Provider == provider
                    && x.IsEnabled
                    && x.Status == PaymentProviderAccountStatus.Active,
                cancellationToken);
    }
}
