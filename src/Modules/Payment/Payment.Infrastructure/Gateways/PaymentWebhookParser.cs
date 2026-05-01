using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Payment.Application.Exceptions;
using Payment.Application.Integrations;
using Payment.Domain.Enums;
using Payment.Infrastructure.Options;

namespace Payment.Infrastructure.Gateways;

public sealed class PaymentWebhookParser : IPaymentWebhookParser
{
    private readonly IyzicoOptions _iyzicoOptions;

    public PaymentWebhookParser(IOptions<IyzicoOptions> iyzicoOptions)
    {
        _iyzicoOptions = iyzicoOptions.Value;
    }

    public Task<ParsedPaymentWebhook> ParseAsync(
        PaymentProvider provider,
        string payload,
        string? signature,
        CancellationToken cancellationToken = default)
    {
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        if (provider == PaymentProvider.Iyzico)
            ValidateIyzicoSignature(root, signature);

        var status = GetString(root, "paymentStatus")
            ?? GetString(root, "status")
            ?? GetString(root, "iyziEventType");

        var idempotencyKey = GetString(root, "token")
            ?? GetString(root, "eventId")
            ?? GetString(root, "iyziPaymentId");

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            idempotencyKey = $"{provider}-webhook-{Guid.NewGuid():N}";

        var refundedAmount = GetDecimal(root, "refundedAmount")
            ?? GetDecimal(root, "refundAmount")
            ?? GetDecimal(root, "price");

        return Task.FromResult(new ParsedPaymentWebhook(
            provider,
            MapOutcome(status, GetNullableInt(root, "fraudStatus")),
            idempotencyKey,
            GetString(root, "paymentReference")
                ?? GetString(root, "paymentId")
                ?? GetString(root, "iyziPaymentId"),
            GetString(root, "paymentConversationId")
                ?? GetString(root, "conversationId"),
            GetString(root, "failureCode") ?? GetString(root, "errorCode"),
            GetString(root, "failureMessage") ?? GetString(root, "errorMessage"),
            refundedAmount));
    }

    private void ValidateIyzicoSignature(JsonElement root, string? signature)
    {
        if (string.IsNullOrWhiteSpace(_iyzicoOptions.SecretKey))
            return;

        if (string.IsNullOrWhiteSpace(signature))
            throw new PaymentWebhookValidationException("Webhook signature is missing.");

        var expected = ComputeHmac(
            _iyzicoOptions.SecretKey,
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

    private static decimal? GetDecimal(JsonElement root, string propertyName)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (!string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (property.Value.ValueKind == JsonValueKind.Number)
                return property.Value.GetDecimal();

            if (property.Value.ValueKind == JsonValueKind.String
                && decimal.TryParse(property.Value.GetString(), out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static int? GetNullableInt(JsonElement root, string propertyName)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (!string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (property.Value.ValueKind == JsonValueKind.Number)
                return property.Value.GetInt32();

            if (property.Value.ValueKind == JsonValueKind.String
                && int.TryParse(property.Value.GetString(), out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static bool EqualsFixedTime(string expected, string actual)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(actual.Trim().ToLowerInvariant()));
    }

    private static PaymentGatewayOutcome MapOutcome(string? status, int? fraudStatus)
    {
        var normalized = status?.Trim().ToLowerInvariant();

        return normalized switch
        {
            "init_checkout_form" or "callback3ds" => PaymentGatewayOutcome.RequiresAction,
            "success" when fraudStatus == 0 => PaymentGatewayOutcome.Authorized,
            "success" => PaymentGatewayOutcome.Captured,
            "refund" or "refunded" => PaymentGatewayOutcome.Refunded,
            "cancel" or "cancelled" => PaymentGatewayOutcome.Cancelled,
            "failed" or "failure" or "error" => PaymentGatewayOutcome.Failed,
            _ => PaymentGatewayOutcome.Failed
        };
    }
}
