using MediatR;
using Payment.Application.Payments.DTOs;

namespace Payment.Application.Payments.Queries.GetStorePaymentById;

public sealed record GetStorePaymentByIdQuery(
    Guid StoreId,
    Guid PaymentId) : IRequest<PaymentDto?>;
