namespace Pricing.Application.Exceptions;

public sealed class DuplicateDefaultPriceListException : ApplicationException
{
    public DuplicateDefaultPriceListException(Guid storeId, string currencyCode)
        : base("An active default price list already exists for this store and currency.")
    {
        StoreId = storeId;
        CurrencyCode = currencyCode;
    }

    public Guid StoreId { get; }
    public string CurrencyCode { get; }
}
