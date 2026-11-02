namespace Payment.Application.Integrations;

public interface IOrderPaymentOperationLock
{
    Task<IAsyncDisposable> AcquireAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default);
}
