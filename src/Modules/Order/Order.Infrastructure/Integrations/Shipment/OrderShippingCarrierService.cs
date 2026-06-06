using Order.Application.Integrations;
using global::Shipment.Contracts;

namespace Order.Infrastructure.Integrations.Shipment;

public sealed class OrderShippingCarrierService : IOrderShippingCarrierService
{
    private readonly IShipmentModuleApi _shipmentModuleApi;

    public OrderShippingCarrierService(IShipmentModuleApi shipmentModuleApi)
    {
        _shipmentModuleApi = shipmentModuleApi;
    }

    public async Task<OrderShippingCarrier?> GetActiveCarrierAsync(
        Guid storeId,
        Guid carrierId,
        CancellationToken cancellationToken = default)
    {
        var carrier = await _shipmentModuleApi.GetActiveShippingCarrierByIdAsync(
            new GetActiveShippingCarrierByIdRequest(storeId, carrierId),
            cancellationToken);

        return carrier is null
            ? null
            : new OrderShippingCarrier(
                carrier.Id,
                carrier.Code,
                carrier.Name,
                carrier.ServiceCode,
                carrier.ServiceName,
                carrier.TrackingUrl);
    }
}
