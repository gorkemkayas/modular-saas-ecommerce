using MediatR;
using Payment.Domain.Enums;

namespace Payment.Application.Payments.Commands.CreatePaymentForOrder;

public sealed record CreatePaymentForOrderCommand(
    Guid StoreId,
    Guid ExternalUserId,
    Guid OrderId,
    PaymentMethodType MethodType) : IRequest<Guid>;
