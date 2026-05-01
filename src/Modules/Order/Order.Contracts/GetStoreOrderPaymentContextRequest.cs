namespace Order.Contracts;

public sealed record GetStoreOrderPaymentContextRequest(
    Guid StoreId,
    Guid OrderId);
