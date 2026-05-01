using MediatR;
using Payment.Application.Common.Models;
using Payment.Application.Payments.DTOs;
using Payment.Domain.Enums;

namespace Payment.Application.Payments.Queries.SearchStorePayments;

public sealed record SearchStorePaymentsQuery(
    Guid StoreId,
    PaymentStatus? Status,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<PaymentSummaryDto>>;
