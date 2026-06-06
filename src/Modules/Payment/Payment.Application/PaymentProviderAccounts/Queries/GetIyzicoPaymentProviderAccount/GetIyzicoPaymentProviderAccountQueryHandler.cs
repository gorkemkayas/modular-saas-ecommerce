using MediatR;
using Payment.Application.Exceptions;
using Payment.Application.PaymentProviderAccounts.DTOs;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.PaymentProviderAccounts.Queries.GetIyzicoPaymentProviderAccount;

public sealed class GetIyzicoPaymentProviderAccountQueryHandler
    : IRequestHandler<GetIyzicoPaymentProviderAccountQuery, IyzicoPaymentProviderAccountDto?>
{
    private readonly IPaymentProviderAccountRepository _repository;

    public GetIyzicoPaymentProviderAccountQueryHandler(IPaymentProviderAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<IyzicoPaymentProviderAccountDto?> Handle(
        GetIyzicoPaymentProviderAccountQuery query,
        CancellationToken cancellationToken)
    {
        if (query.StoreId == Guid.Empty)
            throw new PaymentValidationException("StoreId is required.");

        var account = await _repository.GetByStoreAndProviderAsync(
            query.StoreId,
            PaymentProvider.Iyzico,
            cancellationToken);

        return account is null ? null : PaymentProviderAccountMapper.ToIyzicoDto(account);
    }
}
