namespace Order.Contracts;

public sealed record GetCustomerOrderShipmentContextRequest(
    Guid StoreId,
    Guid ExternalUserId,
    Guid OrderId);
