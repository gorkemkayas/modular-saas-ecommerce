using Payment.Domain.Common;
using Payment.Domain.Enums;
using Payment.Domain.Exceptions;

namespace Payment.Domain.Entities;

public sealed class PaymentProviderAccount : IAggregateRoot
{
    private PaymentProviderAccount()
    {
    }

    private PaymentProviderAccount(Guid id, Guid storeId, PaymentProvider provider)
    {
        if (storeId == Guid.Empty)
            throw new PaymentDomainException("Store id is required.");

        Id = id;
        StoreId = storeId;
        Provider = provider;
        Status = PaymentProviderAccountStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public PaymentProvider Provider { get; private set; }
    public PaymentProviderAccountStatus Status { get; private set; }
    public bool IsEnabled { get; private set; }
    public string ApiKeyCipherText { get; private set; } = default!;
    public string SecretKeyCipherText { get; private set; } = default!;
    public string ApiKeyLastFour { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public bool IsReadyForPayments =>
        IsEnabled
        && Status == PaymentProviderAccountStatus.Active
        && !string.IsNullOrWhiteSpace(ApiKeyCipherText)
        && !string.IsNullOrWhiteSpace(SecretKeyCipherText);

    public static PaymentProviderAccount CreateIyzico(
        Guid storeId,
        string apiKeyCipherText,
        string secretKeyCipherText,
        string apiKeyLastFour,
        bool isEnabled)
    {
        var account = new PaymentProviderAccount(Guid.NewGuid(), storeId, PaymentProvider.Iyzico);
        account.ConfigureIyzico(
            apiKeyCipherText,
            secretKeyCipherText,
            apiKeyLastFour,
            isEnabled);

        return account;
    }

    public void ConfigureIyzico(
        string apiKeyCipherText,
        string secretKeyCipherText,
        string apiKeyLastFour,
        bool isEnabled)
    {
        if (Provider != PaymentProvider.Iyzico)
            throw new PaymentDomainException("Only Iyzico accounts can be configured with Iyzico settings.");

        ApiKeyCipherText = NormalizeRequired(apiKeyCipherText, "Api key", 4_000);
        SecretKeyCipherText = NormalizeRequired(secretKeyCipherText, "Secret key", 4_000);
        ApiKeyLastFour = NormalizeRequired(apiKeyLastFour, "Api key last four", 8);
        IsEnabled = isEnabled;
        Status = isEnabled
            ? PaymentProviderAccountStatus.Active
            : PaymentProviderAccountStatus.Draft;

        Touch();
    }

    public void Disable()
    {
        IsEnabled = false;
        Status = PaymentProviderAccountStatus.Disabled;
        Touch();
    }

    public void Enable()
    {
        if (string.IsNullOrWhiteSpace(ApiKeyCipherText)
            || string.IsNullOrWhiteSpace(SecretKeyCipherText))
        {
            throw new PaymentDomainException("Payment provider account is missing required credentials.");
        }

        IsEnabled = true;
        Status = PaymentProviderAccountStatus.Active;
        Touch();
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
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
}
