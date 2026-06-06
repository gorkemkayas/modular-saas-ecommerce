using Payment.Domain.Enums;

namespace Payment.Application.PaymentProviderAccounts.DTOs;

public sealed record IyzicoPaymentProviderAccountDto(
    Guid Id,
    Guid StoreId,
    PaymentProvider Provider,
    PaymentProviderAccountStatus Status,
    bool IsEnabled,
    bool IsReadyForPayments,
    string? ApiKeyMasked,
    bool HasSecretKey,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
