using Order.Application.Exceptions;
using Order.Application.Integrations;
using Payment.Contracts;
using Payment.Domain.Enums;

namespace Order.Infrastructure.Integrations.Payment;

public sealed class OrderCancellationPaymentGuard(IPaymentModuleApi payments) : IOrderCancellationPaymentGuard
{
    public async Task EnsureCanCancelAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default)
    {
        // Called while the Order operation lock is owned. A failed read must not allow cancellation;
        // a successful read returning no payment means payment processing has not started.
        var payment = await payments.GetByOrderIdAsync(new GetPaymentByOrderIdRequest(storeId, orderId), cancellationToken);
        if (payment is { Status: PaymentStatus.Pending, HasProviderAttempt: false })
            return;
        if (payment is not null && payment.Status is not (PaymentStatus.Failed or PaymentStatus.Cancelled))
            throw new OrderCancellationNotAllowedException();
    }
}
