namespace Order.Application.Exceptions;

public sealed class OrderPricingUnavailableException : ApplicationException
{
    public OrderPricingUnavailableException(Guid productId, Guid? productVariantId, string currencyCode)
        : base("Pricing could not be resolved for the requested order item.")
    {
        ProductId = productId;
        ProductVariantId = productVariantId;
        CurrencyCode = currencyCode;
    }

    public Guid ProductId { get; }
    public Guid? ProductVariantId { get; }
    public string CurrencyCode { get; }
}
