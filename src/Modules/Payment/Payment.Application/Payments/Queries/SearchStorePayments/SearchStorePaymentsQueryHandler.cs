using MediatR;
using Payment.Application.Abstractions.Queries;
using Payment.Application.Common.Models;
using Payment.Application.Payments.DTOs;

namespace Payment.Application.Payments.Queries.SearchStorePayments;

public sealed class SearchStorePaymentsQueryHandler : IRequestHandler<SearchStorePaymentsQuery, PagedResult<PaymentSummaryDto>>
{
    private readonly IPaymentReadService _paymentReadService;

    public SearchStorePaymentsQueryHandler(IPaymentReadService paymentReadService)
    {
        _paymentReadService = paymentReadService;
    }

    public Task<PagedResult<PaymentSummaryDto>> Handle(SearchStorePaymentsQuery query, CancellationToken cancellationToken)
    {
        return _paymentReadService.SearchAsync(
            query.StoreId,
            query.Status,
            query.PageNumber,
            query.PageSize,
            cancellationToken);
    }
}
