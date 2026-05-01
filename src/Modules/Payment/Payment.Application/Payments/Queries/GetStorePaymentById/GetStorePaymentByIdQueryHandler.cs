using MediatR;
using Payment.Application.Abstractions.Queries;
using Payment.Application.Payments.DTOs;

namespace Payment.Application.Payments.Queries.GetStorePaymentById;

public sealed class GetStorePaymentByIdQueryHandler : IRequestHandler<GetStorePaymentByIdQuery, PaymentDto?>
{
    private readonly IPaymentReadService _paymentReadService;

    public GetStorePaymentByIdQueryHandler(IPaymentReadService paymentReadService)
    {
        _paymentReadService = paymentReadService;
    }

    public Task<PaymentDto?> Handle(GetStorePaymentByIdQuery query, CancellationToken cancellationToken)
    {
        return _paymentReadService.GetByIdAsync(query.StoreId, query.PaymentId, cancellationToken);
    }
}
