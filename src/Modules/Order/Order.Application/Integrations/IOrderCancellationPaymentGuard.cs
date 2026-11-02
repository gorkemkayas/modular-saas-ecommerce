namespace Order.Application.Integrations;

public interface IOrderCancellationPaymentGuard
{
    Task EnsureCanCancelAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default);
}
