namespace Catalog.Application.Exceptions;

public sealed class CatalogQuotaExceededException : ApplicationException
{
    public CatalogQuotaExceededException(
        Guid storeId,
        string quotaKey,
        int currentCount,
        int limit)
        : base($"Catalog quota '{quotaKey}' exceeded. Current count is {currentCount}, limit is {limit}.")
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
