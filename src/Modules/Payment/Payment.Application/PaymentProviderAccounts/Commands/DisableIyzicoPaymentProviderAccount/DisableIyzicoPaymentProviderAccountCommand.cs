using MediatR;
using Payment.Application.PaymentProviderAccounts.DTOs;

namespace Payment.Application.PaymentProviderAccounts.Commands.DisableIyzicoPaymentProviderAccount;

public sealed record DisableIyzicoPaymentProviderAccountCommand(Guid StoreId)
    : IRequest<IyzicoPaymentProviderAccountDto>;
