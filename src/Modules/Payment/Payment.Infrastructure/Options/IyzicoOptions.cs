using Payment.Domain.Enums;

namespace Payment.Infrastructure.Options;

public sealed class IyzicoOptions
{
    public const string SectionName = "Modules:Payment:Iyzico";

    public PaymentProviderEnvironment Environment { get; init; } = PaymentProviderEnvironment.Sandbox;
    public string? BaseUrl { get; init; }
    public string Locale { get; init; } = "tr";
    public string CallbackUrl { get; init; } = string.Empty;
    public string? DefaultBuyerIdentityNumber { get; init; }
    public string InitializeCheckoutFormPath { get; init; } = "/payment/iyzipos/checkoutform/initialize/auth/ecom";
    public string RetrieveCheckoutFormPath { get; init; } = "/payment/iyzipos/checkoutform/auth/ecom/detail";
    public string CapturePath { get; init; } = "/payment/capture";
    public string CancelPath { get; init; } = "/payment/cancel";
    public string RefundPath { get; init; } = "/payment/refund";
}
