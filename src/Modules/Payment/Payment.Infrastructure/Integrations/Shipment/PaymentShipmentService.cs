using Payment.Application.Integrations;
using Shipment.Contracts;

namespace Payment.Infrastructure.Integrations.Shipment;

public sealed class PaymentShipmentService : IShipmentPaymentService
{
    private readonly IShipmentModuleApi _shipmentModuleApi;

    public PaymentShipmentService(IShipmentModuleApi shipmentModuleApi)
    {
        _shipmentModuleApi = shipmentModuleApi;
    }

    public async Task EnsureShipmentCreatedForCapturedOrderAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        await _shipmentModuleApi.EnsureShipmentCreatedForCapturedOrderAsync(
            new EnsureShipmentCreatedForCapturedOrderRequest(storeId, orderId),
            cancellationToken);
    }
}
