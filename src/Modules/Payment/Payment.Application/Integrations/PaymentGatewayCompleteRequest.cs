namespace Payment.Application.Integrations;

public sealed record PaymentGatewayCompleteRequest(
    string Token,
    Guid StoreId = default,
    Guid? ProviderAccountId = null);
