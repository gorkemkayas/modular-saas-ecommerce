namespace Shipment.Contracts;

public sealed record GetStoreShipmentByIdRequest(
    Guid StoreId,
    Guid ShipmentId);
