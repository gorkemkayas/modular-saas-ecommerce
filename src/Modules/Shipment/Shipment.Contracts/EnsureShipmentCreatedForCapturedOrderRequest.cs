namespace Shipment.Contracts;

public sealed record EnsureShipmentCreatedForCapturedOrderRequest(
    Guid StoreId,
    Guid OrderId,
    string? InternalNote = null);
