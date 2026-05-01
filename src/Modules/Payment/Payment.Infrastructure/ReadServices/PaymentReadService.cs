using Microsoft.EntityFrameworkCore;
using Payment.Application.Abstractions.Queries;
using Payment.Application.Common.Models;
using Payment.Application.Payments.DTOs;
using Payment.Domain.Enums;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.ReadServices;

public sealed class PaymentReadService : IPaymentReadService
{
    private readonly PaymentDbContext _context;

    public PaymentReadService(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid storeId, Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(x => x.Attempts)
            .Include(x => x.Refunds)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == paymentId, cancellationToken);

        return payment is null ? null : Map(payment);
    }

    public async Task<PaymentDto?> GetByOrderIdAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(x => x.Attempts)
            .Include(x => x.Refunds)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.OrderId == orderId, cancellationToken);

        return payment is null ? null : Map(payment);
    }

    public async Task<PagedResult<PaymentSummaryDto>> SearchAsync(
        Guid storeId,
        PaymentStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Where(x => x.StoreId == storeId);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PaymentSummaryDto(
                x.Id,
                x.OrderId,
                x.OrderNumber,
                x.Amount,
                x.CurrencyCode,
                x.Status,
                x.Provider,
                x.MethodType,
                x.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<PaymentSummaryDto>(items, pageNumber, pageSize, totalCount);
    }

    private static PaymentDto Map(Payment.Domain.Entities.Payment payment)
    {
        return new PaymentDto(
            payment.Id,
            payment.StoreId,
            payment.OrderId,
            payment.OrderNumber,
            payment.CustomerId,
            payment.Amount,
            payment.CurrencyCode,
            payment.Status,
            payment.Provider,
            payment.MethodType,
            payment.ExternalPaymentReference,
            payment.ExternalConversationId,
            payment.FailureCode,
            payment.FailureMessage,
            payment.AuthorizedAtUtc,
            payment.CapturedAtUtc,
            payment.CancelledAtUtc,
            payment.FailedAtUtc,
            payment.RefundedAmount,
            payment.CreatedAtUtc,
            payment.UpdatedAtUtc,
            payment.Attempts
                .OrderBy(x => x.AttemptNumber)
                .Select(x => new PaymentAttemptDto(
                    x.Id,
                    x.AttemptNumber,
                    x.OperationType,
                    x.Status,
                    x.IdempotencyKey,
                    x.ProviderRequestReference,
                    x.ProviderTransactionReference,
                    x.FailureCode,
                    x.FailureMessage,
                    x.ProcessedAtUtc))
                .ToArray(),
            payment.Refunds
                .OrderBy(x => x.CreatedAtUtc)
                .Select(x => new PaymentRefundDto(
                    x.Id,
                    x.Amount,
                    x.Reason,
                    x.ProviderRefundReference,
                    x.CreatedAtUtc))
                .ToArray());
    }
}
