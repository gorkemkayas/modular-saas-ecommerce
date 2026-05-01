namespace Payment.Application.Integrations;

public enum PaymentGatewayOutcome
{
    RequiresAction = 0,
    Authorized = 1,
    Captured = 2,
    Cancelled = 3,
    Refunded = 4,
    Failed = 5
}
