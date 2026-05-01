using Payment.Domain.Exceptions;

namespace Payment.Domain.Entities;

public sealed class PaymentRefund
{
    private PaymentRefund()
    {
    }

    private PaymentRefund(
        Guid id,
        Guid paymentId,
        decimal amount,
        string reason,
        string? providerRefundReference)
    {
        if (paymentId == Guid.Empty)
            throw new PaymentDomainException("Payment id is required.");

        if (amount <= 0)
            throw new PaymentDomainException("Refund amount must be greater than zero.");

        Id = id;
        PaymentId = paymentId;
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Reason = NormalizeRequired(reason, "Refund reason", 500);
        ProviderRefundReference = NormalizeOptional(providerRefundReference, 200);
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; } = default!;
    public string? ProviderRefundReference { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static PaymentRefund Create(
        Guid paymentId,
        decimal amount,
        string reason,
        string? providerRefundReference)
    {
        return new PaymentRefund(
            Guid.NewGuid(),
            paymentId,
            amount,
            reason,
            providerRefundReference);
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PaymentDomainException($"{fieldName} is required.");

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new PaymentDomainException($"{fieldName} cannot exceed {maxLength} characters.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new PaymentDomainException($"Value cannot exceed {maxLength} characters.");

        return normalized;
    }
}
