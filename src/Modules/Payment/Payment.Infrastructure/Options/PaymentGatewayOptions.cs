namespace Payment.Infrastructure.Options;

public sealed class PaymentGatewayOptions
{
    public const string SectionName = "Modules:Payment:Gateway";

    public string Provider { get; init; } = "Mock";
}
