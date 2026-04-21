namespace Pricing.Application.Exceptions;

public sealed class PriceListNotFoundException : ApplicationException
{
    public PriceListNotFoundException(Guid priceListId)
        : base("Price list was not found.")
    {
        PriceListId = priceListId;
    }

    public Guid PriceListId { get; }
}
