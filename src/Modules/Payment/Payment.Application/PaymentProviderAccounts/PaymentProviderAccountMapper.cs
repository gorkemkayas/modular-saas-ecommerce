using Payment.Application.PaymentProviderAccounts.DTOs;
using Payment.Domain.Entities;

namespace Payment.Application.PaymentProviderAccounts;

internal static class PaymentProviderAccountMapper
{
    public static IyzicoPaymentProviderAccountDto ToIyzicoDto(PaymentProviderAccount account)
    {
        return new IyzicoPaymentProviderAccountDto(
            account.Id,
            account.StoreId,
            account.Provider,
            account.Status,
            account.IsEnabled,
            account.IsReadyForPayments,
            MaskLastFour(account.ApiKeyLastFour),
            !string.IsNullOrWhiteSpace(account.SecretKeyCipherText),
            account.CreatedAtUtc,
            account.UpdatedAtUtc);
    }

    private static string? MaskLastFour(string? lastFour)
    {
        return string.IsNullOrWhiteSpace(lastFour)
            ? null
            : $"****{lastFour.Trim()}";
    }
}
