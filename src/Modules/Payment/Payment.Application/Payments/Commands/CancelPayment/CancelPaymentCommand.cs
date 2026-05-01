using MediatR;

namespace Payment.Application.Payments.Commands.CancelPayment;

public sealed record CancelPaymentCommand(
    Guid StoreId,
    Guid PaymentId,
    string IdempotencyKey) : IRequest;
