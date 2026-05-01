using MediatR;
using Payment.Application.Payments.DTOs;

namespace Payment.Application.Payments.Queries.GetMyPaymentByOrderId;

public sealed record GetMyPaymentByOrderIdQuery(
    Guid StoreId,
    Guid ExternalUserId,
    Guid OrderId) : IRequest<PaymentDto?>;
