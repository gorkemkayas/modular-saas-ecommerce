namespace Shipment.Contracts;

public sealed record GetStoreOrderShipmentsRequest(
    Guid StoreId,
    Guid OrderId);
