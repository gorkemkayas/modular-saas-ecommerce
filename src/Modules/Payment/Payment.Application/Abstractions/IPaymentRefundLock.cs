namespace Payment.Application.Abstractions;

public interface IPaymentRefundLock
{
    Task<IAsyncDisposable> AcquireAsync(Guid storeId, Guid paymentId, CancellationToken cancellationToken = default);
}
