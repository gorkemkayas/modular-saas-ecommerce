using PaymentEntity = Payment.Domain.Entities.Payment;

namespace Payment.Domain.Repositories;

public interface IPaymentRepository
{
    Task AddAsync(PaymentEntity payment, CancellationToken cancellationToken = default);
    Task<PaymentEntity?> GetByIdAsync(Guid storeId, Guid paymentId, CancellationToken cancellationToken = default);
    Task<PaymentEntity?> GetByOrderIdAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default);
    Task<PaymentEntity?> GetByProviderReferenceAsync(
        Payment.Domain.Enums.PaymentProvider provider,
        string? externalConversationId,
        string? externalPaymentReference,
        string? providerRequestReference,
        CancellationToken cancellationToken = default);

    Task<PaymentEntity?> GetByProviderRequestReferenceAsync(
        Payment.Domain.Enums.PaymentProvider provider,
        string providerRequestReference,
        CancellationToken cancellationToken = default);
}
