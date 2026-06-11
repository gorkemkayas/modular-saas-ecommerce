namespace Catalog.Application.Exceptions;

public sealed class CatalogFeatureUnavailableException : ApplicationException
{
    public CatalogFeatureUnavailableException(Guid storeId, string featureKey)
        : base($"Catalog feature '{featureKey}' is not available for this tenant subscription.")
    {
        StoreId = storeId;
        FeatureKey = featureKey;
    }

    public Guid StoreId { get; }
    public string FeatureKey { get; }
}
