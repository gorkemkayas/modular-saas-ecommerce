using Payment.Domain.Enums;

namespace Payment.Application.Integrations;

public interface IPaymentWebhookParser
{
    Task<ParsedPaymentWebhook> ParseAsync(
        PaymentProvider provider,
        string payload,
        string? signature,
        CancellationToken cancellationToken = default);
}
