using Payment.Application.Common.Models;
using Payment.Application.Payments.DTOs;
using Payment.Domain.Enums;

namespace Payment.Application.Abstractions.Queries;

public interface IPaymentReadService
{
    Task<PaymentDto?> GetByIdAsync(Guid storeId, Guid paymentId, CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetByOrderIdAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default);
    Task<PagedResult<PaymentSummaryDto>> SearchAsync(
        Guid storeId,
        PaymentStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
