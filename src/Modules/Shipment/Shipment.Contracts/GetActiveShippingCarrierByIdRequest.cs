namespace Shipment.Contracts;

public sealed record GetActiveShippingCarrierByIdRequest(
    Guid StoreId,
    Guid CarrierId);
