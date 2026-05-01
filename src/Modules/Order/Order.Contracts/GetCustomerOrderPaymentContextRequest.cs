namespace Order.Contracts;

public sealed record GetCustomerOrderPaymentContextRequest(
    Guid StoreId,
    Guid ExternalUserId,
    Guid OrderId);
