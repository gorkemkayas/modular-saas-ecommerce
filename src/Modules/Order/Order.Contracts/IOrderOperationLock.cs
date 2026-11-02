namespace Order.Contracts;

public interface IOrderOperationLock
{
    Task<IAsyncDisposable> AcquireAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default);
}
