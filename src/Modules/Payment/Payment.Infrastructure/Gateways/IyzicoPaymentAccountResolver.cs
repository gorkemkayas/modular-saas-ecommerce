using Microsoft.Extensions.Options;
using Payment.Application.Abstractions;
using Payment.Application.Exceptions;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;
using Payment.Infrastructure.Options;

namespace Payment.Infrastructure.Gateways;

public interface IIyzicoPaymentAccountResolver
{
    Task<ResolvedIyzicoPaymentAccount> ResolveAsync(
        Guid storeId,
        Guid? providerAccountId,
        CancellationToken cancellationToken = default);
}

internal sealed class IyzicoPaymentAccountResolver : IIyzicoPaymentAccountResolver
{
    private const string SandboxBaseUrl = "https://sandbox-api.iyzipay.com";
    private const string ProductionBaseUrl = "https://api.iyzipay.com";

    private readonly IPaymentProviderAccountRepository _repository;
    private readonly IPaymentCredentialProtector _credentialProtector;
    private readonly IyzicoOptions _options;

    public IyzicoPaymentAccountResolver(
        IPaymentProviderAccountRepository repository,
        IPaymentCredentialProtector credentialProtector,
        IOptions<IyzicoOptions> options)
    {
        _repository = repository;
        _credentialProtector = credentialProtector;
        _options = options.Value;
    }

    public async Task<ResolvedIyzicoPaymentAccount> ResolveAsync(
        Guid storeId,
        Guid? providerAccountId,
        CancellationToken cancellationToken = default)
    {
        if (storeId == Guid.Empty)
            throw new PaymentProviderAccountNotConfiguredException("Store id is required for Iyzico account resolution.");

        var account = providerAccountId.HasValue
            ? await _repository.GetByIdAsync(storeId, providerAccountId.Value, cancellationToken)
            : await _repository.GetReadyForPaymentsAsync(storeId, PaymentProvider.Iyzico, cancellationToken);

        if (account is null)
        {
            throw new PaymentProviderAccountNotConfiguredException(
                "Iyzico account is not configured or enabled for this store.");
        }

        if (!providerAccountId.HasValue && !account.IsReadyForPayments)
        {
            throw new PaymentProviderAccountNotConfiguredException(
                "Iyzico account is not configured or enabled for this store.");
        }

        if (string.IsNullOrWhiteSpace(account.ApiKeyCipherText)
            || string.IsNullOrWhiteSpace(account.SecretKeyCipherText))
        {
            throw new PaymentProviderAccountNotConfiguredException("Iyzico account credentials are missing.");
        }

        string apiKey;
        string secretKey;

        try
        {
            apiKey = _credentialProtector.Unprotect(account.ApiKeyCipherText);
            secretKey = _credentialProtector.Unprotect(account.SecretKeyCipherText);
        }
        catch (Exception exception)
        {
            throw new PaymentProviderAccountNotConfiguredException(
                $"Iyzico account credentials could not be decrypted: {exception.Message}");
        }

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(secretKey))
            throw new PaymentProviderAccountNotConfiguredException("Iyzico account credentials are missing.");

        var callbackUrl = _options.CallbackUrl;
        if (string.IsNullOrWhiteSpace(callbackUrl))
            throw new PaymentProviderAccountNotConfiguredException("Iyzico callback URL is not configured.");

        var defaultBuyerIdentityNumber = _options.DefaultBuyerIdentityNumber;
        if (string.IsNullOrWhiteSpace(defaultBuyerIdentityNumber))
            throw new PaymentProviderAccountNotConfiguredException("Iyzico default buyer identity number is not configured.");

        return new ResolvedIyzicoPaymentAccount(
            account.Id,
            account.StoreId,
            _options.Environment,
            ResolveBaseUrl(),
            apiKey,
            secretKey,
            _options.Locale,
            callbackUrl,
            defaultBuyerIdentityNumber,
            _options.InitializeCheckoutFormPath,
            _options.RetrieveCheckoutFormPath,
            _options.CapturePath,
            _options.CancelPath,
            _options.RefundPath);
    }

    private string ResolveBaseUrl()
    {
        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
            return _options.BaseUrl;

        return _options.Environment == PaymentProviderEnvironment.Production
            ? ProductionBaseUrl
            : SandboxBaseUrl;
    }
}

public sealed record ResolvedIyzicoPaymentAccount(
    Guid AccountId,
    Guid StoreId,
    PaymentProviderEnvironment Environment,
    string BaseUrl,
    string ApiKey,
    string SecretKey,
    string Locale,
    string CallbackUrl,
    string DefaultBuyerIdentityNumber,
    string InitializeCheckoutFormPath,
    string RetrieveCheckoutFormPath,
    string CapturePath,
    string CancelPath,
    string RefundPath);
