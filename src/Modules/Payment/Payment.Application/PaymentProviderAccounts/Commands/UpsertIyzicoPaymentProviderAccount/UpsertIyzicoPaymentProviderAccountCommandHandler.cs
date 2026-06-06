using MediatR;
using Payment.Application.Abstractions;
using Payment.Application.Exceptions;
using Payment.Application.PaymentProviderAccounts.DTOs;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.PaymentProviderAccounts.Commands.UpsertIyzicoPaymentProviderAccount;

public sealed class UpsertIyzicoPaymentProviderAccountCommandHandler
    : IRequestHandler<UpsertIyzicoPaymentProviderAccountCommand, IyzicoPaymentProviderAccountDto>
{
    private readonly IPaymentProviderAccountRepository _repository;
    private readonly IPaymentCredentialProtector _credentialProtector;
    private readonly IUnitOfWork _unitOfWork;

    public UpsertIyzicoPaymentProviderAccountCommandHandler(
        IPaymentProviderAccountRepository repository,
        IPaymentCredentialProtector credentialProtector,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _credentialProtector = credentialProtector;
        _unitOfWork = unitOfWork;
    }

    public async Task<IyzicoPaymentProviderAccountDto> Handle(
        UpsertIyzicoPaymentProviderAccountCommand command,
        CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty)
            throw new PaymentValidationException("StoreId is required.");

        var existingAccount = await _repository.GetByStoreAndProviderAsync(
            command.StoreId,
            PaymentProvider.Iyzico,
            cancellationToken);

        var apiKeyCipherText = ResolveProtectedCredential(
            command.ApiKey,
            existingAccount?.ApiKeyCipherText,
            "Api key");
        var secretKeyCipherText = ResolveProtectedCredential(
            command.SecretKey,
            existingAccount?.SecretKeyCipherText,
            "Secret key");
        var apiKeyLastFour = ResolveLastFour(command.ApiKey, existingAccount?.ApiKeyLastFour, "Api key");

        PaymentProviderAccount account;

        if (existingAccount is null)
        {
            account = PaymentProviderAccount.CreateIyzico(
                command.StoreId,
                apiKeyCipherText,
                secretKeyCipherText,
                apiKeyLastFour,
                command.IsEnabled);

            await _repository.AddAsync(account, cancellationToken);
        }
        else
        {
            account = existingAccount;
            account.ConfigureIyzico(
                apiKeyCipherText,
                secretKeyCipherText,
                apiKeyLastFour,
                command.IsEnabled);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PaymentProviderAccountMapper.ToIyzicoDto(account);
    }

    private string ResolveProtectedCredential(
        string? rawValue,
        string? existingProtectedValue,
        string fieldName)
    {
        if (!string.IsNullOrWhiteSpace(rawValue))
            return _credentialProtector.Protect(rawValue.Trim());

        if (!string.IsNullOrWhiteSpace(existingProtectedValue))
            return existingProtectedValue;

        throw new PaymentValidationException($"{fieldName} is required.");
    }

    private static string ResolveLastFour(string? rawValue, string? existingLastFour, string fieldName)
    {
        if (!string.IsNullOrWhiteSpace(rawValue))
        {
            var normalized = rawValue.Trim();
            return normalized.Length <= 4 ? normalized : normalized[^4..];
        }

        if (!string.IsNullOrWhiteSpace(existingLastFour))
            return existingLastFour;

        throw new PaymentValidationException($"{fieldName} is required.");
    }
}
