namespace Payment.Domain.Enums;

public enum PaymentOperationType
{
    Authorize = 0,
    Capture = 1,
    Cancel = 2,
    Refund = 3,
    Webhook = 4
}
