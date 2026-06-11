using Payment.Domain.Enums;

namespace Payment.Application.Integrations;

public interface IPaymentWebhookSignatureValidator
{
    Task ValidateAsync(
        PaymentProvider provider,
        string payload,
        string? signature,
        Guid storeId,
        Guid? providerAccountId,
        CancellationToken cancellationToken = default);
}
