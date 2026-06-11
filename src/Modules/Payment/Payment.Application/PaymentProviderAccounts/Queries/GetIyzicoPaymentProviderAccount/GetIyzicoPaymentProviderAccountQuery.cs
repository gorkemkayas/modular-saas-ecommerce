using MediatR;
using Payment.Application.PaymentProviderAccounts.DTOs;

namespace Payment.Application.PaymentProviderAccounts.Queries.GetIyzicoPaymentProviderAccount;

public sealed record GetIyzicoPaymentProviderAccountQuery(Guid StoreId)
    : IRequest<IyzicoPaymentProviderAccountDto?>;
