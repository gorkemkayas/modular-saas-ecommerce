using MediatR;
using Payment.Application.Abstractions.Queries;
using Payment.Application.Integrations;
using Payment.Application.Payments.DTOs;

namespace Payment.Application.Payments.Queries.GetMyPaymentByOrderId;

public sealed class GetMyPaymentByOrderIdQueryHandler : IRequestHandler<GetMyPaymentByOrderIdQuery, PaymentDto?>
{
    private readonly IPaymentReadService _paymentReadService;
    private readonly IOrderPaymentContextService _orderPaymentContextService;

    public GetMyPaymentByOrderIdQueryHandler(
        IPaymentReadService paymentReadService,
        IOrderPaymentContextService orderPaymentContextService)
    {
        _paymentReadService = paymentReadService;
        _orderPaymentContextService = orderPaymentContextService;
    }

    public async Task<PaymentDto?> Handle(GetMyPaymentByOrderIdQuery query, CancellationToken cancellationToken)
    {
        var orderContext = await _orderPaymentContextService.GetCustomerOrderContextAsync(
            query.StoreId,
            query.ExternalUserId,
            query.OrderId,
            cancellationToken);

        if (orderContext is null)
            return null;

        return await _paymentReadService.GetByOrderIdAsync(
            query.StoreId,
            query.OrderId,
            cancellationToken);
    }
}
