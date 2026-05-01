using MediatR;
using Payment.Application.Payments.DTOs;

namespace Payment.Application.Payments.Commands.AuthorizePayment;

public sealed record AuthorizePaymentCommand(
    Guid StoreId,
    Guid ExternalUserId,
    Guid OrderId,
    string IdempotencyKey,
    string? ClientIpAddress) : IRequest<PaymentActionResultDto>;
