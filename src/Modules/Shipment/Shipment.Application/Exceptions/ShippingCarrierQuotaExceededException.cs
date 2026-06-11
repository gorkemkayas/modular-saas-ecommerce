namespace Shipment.Application.Exceptions;

public sealed class ShippingCarrierQuotaExceededException : ApplicationException
{
    public ShippingCarrierQuotaExceededException(
        Guid storeId,
        string quotaKey,
        int currentCount,
        int limit)
        : base($"Shipping carrier quota '{quotaKey}' exceeded. Current count is {currentCount}, limit is {limit}.")
    {
        StoreId = storeId;
        QuotaKey = quotaKey;
        CurrentCount = currentCount;
        Limit = limit;
    }

    public Guid StoreId { get; }
    public string QuotaKey { get; }
    public int CurrentCount { get; }
    public int Limit { get; }
}
