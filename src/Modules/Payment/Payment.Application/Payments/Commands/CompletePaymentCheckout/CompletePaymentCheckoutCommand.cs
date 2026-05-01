using MediatR;
using Payment.Application.Payments.DTOs;
using Payment.Domain.Enums;

namespace Payment.Application.Payments.Commands.CompletePaymentCheckout;

public sealed record CompletePaymentCheckoutCommand(
    PaymentProvider Provider,
    string Token) : IRequest<PaymentActionResultDto>;
