namespace Payment.Infrastructure.Options;

public sealed class IyzicoOptions
{
    public const string SectionName = "Modules:Payment:Iyzico";

    public string BaseUrl { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string Locale { get; init; } = "tr";
    public string CallbackUrl { get; init; } = string.Empty;
    public string DefaultBuyerIdentityNumber { get; init; } = string.Empty;
    public string InitializeCheckoutFormPath { get; init; } = "/payment/iyzipos/checkoutform/initialize/auth/ecom";
    public string RetrieveCheckoutFormPath { get; init; } = "/payment/iyzipos/checkoutform/auth/ecom/detail";
    public string CapturePath { get; init; } = "/payment/capture";
    public string CancelPath { get; init; } = "/payment/cancel";
    public string RefundPath { get; init; } = "/payment/refund";
}
