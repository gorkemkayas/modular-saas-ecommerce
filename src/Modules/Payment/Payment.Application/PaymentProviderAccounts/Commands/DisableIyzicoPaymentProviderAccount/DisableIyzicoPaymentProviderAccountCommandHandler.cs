using MediatR;
using Payment.Application.Abstractions;
using Payment.Application.Exceptions;
using Payment.Application.PaymentProviderAccounts.DTOs;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.PaymentProviderAccounts.Commands.DisableIyzicoPaymentProviderAccount;

public sealed class DisableIyzicoPaymentProviderAccountCommandHandler
    : IRequestHandler<DisableIyzicoPaymentProviderAccountCommand, IyzicoPaymentProviderAccountDto>
{
    private readonly IPaymentProviderAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DisableIyzicoPaymentProviderAccountCommandHandler(
        IPaymentProviderAccountRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IyzicoPaymentProviderAccountDto> Handle(
        DisableIyzicoPaymentProviderAccountCommand command,
        CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty)
            throw new PaymentValidationException("StoreId is required.");

        var account = await _repository.GetByStoreAndProviderAsync(
            command.StoreId,
            PaymentProvider.Iyzico,
            cancellationToken);

        if (account is null)
            throw new PaymentProviderAccountNotConfiguredException("Iyzico account is not configured for this store.");

        account.Disable();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PaymentProviderAccountMapper.ToIyzicoDto(account);
    }
}
