using MediatR;
using Payment.Application.Payments.DTOs;

namespace Payment.Application.Payments.Commands.CapturePayment;

public sealed record CapturePaymentCommand(
    Guid StoreId,
    Guid PaymentId,
    string IdempotencyKey) : IRequest<PaymentActionResultDto>;
