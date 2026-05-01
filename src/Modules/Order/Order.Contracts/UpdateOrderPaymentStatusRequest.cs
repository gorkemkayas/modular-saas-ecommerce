namespace Order.Contracts;

public sealed record UpdateOrderPaymentStatusRequest(
    Guid StoreId,
    Guid OrderId,
    string? PaymentReference);
