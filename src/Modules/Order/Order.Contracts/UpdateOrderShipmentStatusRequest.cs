namespace Order.Contracts;

public sealed record UpdateOrderShipmentStatusRequest(
    Guid StoreId,
    Guid OrderId,
    string ShipmentReference);
