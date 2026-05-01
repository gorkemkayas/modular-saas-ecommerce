namespace Order.Contracts;

public sealed record GetStoreOrderShipmentContextRequest(
    Guid StoreId,
    Guid OrderId);
