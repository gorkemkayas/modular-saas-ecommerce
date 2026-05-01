namespace Payment.Contracts;

public sealed record GetPaymentByOrderIdRequest(
    Guid StoreId,
    Guid OrderId);
