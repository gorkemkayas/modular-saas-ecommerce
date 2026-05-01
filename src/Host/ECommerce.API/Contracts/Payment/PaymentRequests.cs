using Payment.Domain.Enums;

namespace ECommerce.API.Contracts.Payment;

public sealed record CreatePaymentRequest(PaymentMethodType MethodType);

public sealed record AuthorizePaymentRequest(string IdempotencyKey);

public sealed record CapturePaymentRequest(string IdempotencyKey);

public sealed record CancelPaymentRequest(string IdempotencyKey);

public sealed record RefundPaymentRequest(
    decimal Amount,
    string Reason,
    string IdempotencyKey);

public sealed record SearchPaymentsRequest(
    PaymentStatus? Status,
    int PageNumber = 1,
    int PageSize = 20);
