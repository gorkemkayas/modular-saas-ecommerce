using Microsoft.AspNetCore.DataProtection;
using Payment.Application.Abstractions;

namespace Payment.Infrastructure.Security;

public sealed class DataProtectionPaymentCredentialProtector : IPaymentCredentialProtector
{
    private readonly IDataProtector _protector;

    public DataProtectionPaymentCredentialProtector(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("Payment.ProviderCredentials.v1");
    }

    public string Protect(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Credential value is required.", nameof(value));

        return _protector.Protect(value.Trim());
    }

    public string Unprotect(string protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue))
            throw new ArgumentException("Protected credential value is required.", nameof(protectedValue));

        return _protector.Unprotect(protectedValue);
    }
}
