using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Domain.Repositories;

public interface IPaymentProviderAccountRepository
{
    Task AddAsync(PaymentProviderAccount account, CancellationToken cancellationToken = default);

    Task<PaymentProviderAccount?> GetByIdAsync(
        Guid storeId,
        Guid accountId,
        CancellationToken cancellationToken = default);

    Task<PaymentProviderAccount?> GetByStoreAndProviderAsync(
        Guid storeId,
        PaymentProvider provider,
        CancellationToken cancellationToken = default);

    Task<PaymentProviderAccount?> GetReadyForPaymentsAsync(
        Guid storeId,
        PaymentProvider provider,
        CancellationToken cancellationToken = default);
}
