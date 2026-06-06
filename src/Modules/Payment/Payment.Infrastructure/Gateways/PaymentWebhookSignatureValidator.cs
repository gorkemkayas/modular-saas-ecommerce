using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Payment.Application.Exceptions;
using Payment.Application.Integrations;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Gateways;

public sealed class PaymentWebhookSignatureValidator : IPaymentWebhookSignatureValidator
{
    private readonly IIyzicoPaymentAccountResolver _iyzicoPaymentAccountResolver;

    public PaymentWebhookSignatureValidator(IIyzicoPaymentAccountResolver iyzicoPaymentAccountResolver)
    {
        _iyzicoPaymentAccountResolver = iyzicoPaymentAccountResolver;
    }

    public async Task ValidateAsync(
        PaymentProvider provider,
        string payload,
        string? signature,
        Guid storeId,
        Guid? providerAccountId,
        CancellationToken cancellationToken = default)
    {
        if (provider != PaymentProvider.Iyzico)
            return;

        if (string.IsNullOrWhiteSpace(signature))
            throw new PaymentWebhookValidationException("Webhook signature is missing.");

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        var account = await _iyzicoPaymentAccountResolver.ResolveAsync(
            storeId,
            providerAccountId,
            cancellationToken);

        var expected = ComputeHmac(
            account.SecretKey,
            GetString(root, "iyziEventType"),
            GetString(root, "iyziPaymentId"),
            GetString(root, "token"),
            GetString(root, "paymentConversationId"),
            GetString(root, "status"));

        if (!EqualsFixedTime(expected, signature.Trim()))
            throw new PaymentWebhookValidationException("Webhook signature validation failed.");
    }

    private static string ComputeHmac(string secretKey, params string?[] values)
    {
        var payload = string.Join(
            ":",
            values.Select(value => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim()));

        var hashBytes = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secretKey),
            Encoding.UTF8.GetBytes(payload));

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static string? GetString(JsonElement root, string propertyName)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                return property.Value.ValueKind == JsonValueKind.String
                    ? property.Value.GetString()
                    : property.Value.ToString();
        }

        return null;
    }

    private static bool EqualsFixedTime(string expected, string actual)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(actual.Trim().ToLowerInvariant()));
    }
}
