using Payment.Domain.Enums;

namespace Payment.Contracts;

public sealed record CreatePaymentForOrderRequest(
    Guid StoreId,
    Guid ExternalUserId,
    Guid OrderId,
    PaymentMethodType MethodType);
