namespace Subscription.Infrastructure.Options;

public sealed class SubscriptionIyzicoOptions
{
    public const string SectionName = "Modules:Subscription:Iyzico";

    public string ApiKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://sandbox-api.iyzipay.com";
    public string Locale { get; init; } = "tr";
    public string CallbackUrl { get; init; } = string.Empty;
    public string DefaultBuyerIdentityNumber { get; init; } = "11111111111";
    public string InitializeCheckoutFormPath { get; init; } = "/payment/iyzipos/checkoutform/initialize/auth/ecom";
    public string RetrieveCheckoutFormPath { get; init; } = "/payment/iyzipos/checkoutform/auth/ecom/detail";
}
