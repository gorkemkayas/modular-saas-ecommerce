using System.Text.Json;
using Payment.Application.Integrations;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Gateways;

public sealed class PaymentWebhookParser : IPaymentWebhookParser
{
    public Task<ParsedPaymentWebhook> ParseAsync(
        PaymentProvider provider,
        string payload,
        string? signature,
        CancellationToken cancellationToken = default)
    {
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

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
