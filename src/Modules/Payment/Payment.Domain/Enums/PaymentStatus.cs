namespace Payment.Domain.Enums;

public enum PaymentStatus
{
    Pending = 0,
    RequiresAction = 1,
    Authorized = 2,
    Captured = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6,
    PartiallyRefunded = 7
}
