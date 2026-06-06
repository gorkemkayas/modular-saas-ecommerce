using MediatR;
using Payment.Application.PaymentProviderAccounts.DTOs;

namespace Payment.Application.PaymentProviderAccounts.Commands.UpsertIyzicoPaymentProviderAccount;

public sealed record UpsertIyzicoPaymentProviderAccountCommand(
    Guid StoreId,
    string? ApiKey,
    string? SecretKey,
    bool IsEnabled) : IRequest<IyzicoPaymentProviderAccountDto>;
