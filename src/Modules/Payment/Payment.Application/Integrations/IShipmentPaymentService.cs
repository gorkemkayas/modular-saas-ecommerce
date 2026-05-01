namespace Payment.Application.Integrations;

public interface IShipmentPaymentService
{
    Task EnsureShipmentCreatedForCapturedOrderAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken = default);
}
