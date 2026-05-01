namespace Shipment.Contracts;

public interface IShipmentModuleApi
{
    Task<Guid> EnsureShipmentCreatedForCapturedOrderAsync(
        EnsureShipmentCreatedForCapturedOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ShipmentSummaryResult>> GetStoreOrderShipmentsAsync(
        GetStoreOrderShipmentsRequest request,
        CancellationToken cancellationToken = default);

    Task<ShipmentDetailResult?> GetStoreShipmentByIdAsync(
        GetStoreShipmentByIdRequest request,
        CancellationToken cancellationToken = default);
}
