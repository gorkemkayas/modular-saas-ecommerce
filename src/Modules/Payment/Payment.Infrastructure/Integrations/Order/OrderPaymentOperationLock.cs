using Order.Contracts;
using Payment.Application.Integrations;

namespace Payment.Infrastructure.Integrations.Order;

public sealed class OrderPaymentOperationLock(IOrderOperationLock operationLock) : IOrderPaymentOperationLock
{
    public Task<IAsyncDisposable> AcquireAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default)
        => operationLock.AcquireAsync(storeId, orderId, cancellationToken);
}
