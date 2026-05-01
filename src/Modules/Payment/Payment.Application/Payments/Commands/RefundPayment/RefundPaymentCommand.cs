using MediatR;
using Payment.Contracts;

namespace Payment.Application.Payments.Commands.RefundPayment;

public sealed record RefundPaymentCommand(
    Guid StoreId,
    Guid PaymentId,
    decimal Amount,
    string Reason,
    string IdempotencyKey) : IRequest<RefundPaymentResult>;
