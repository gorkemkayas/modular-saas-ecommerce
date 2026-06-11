namespace Store.Application.Exceptions;

public sealed class StoreFeatureUnavailableException : ApplicationException
{
    public StoreFeatureUnavailableException(Guid tenantId, string featureKey)
        : base($"Store feature '{featureKey}' is not available for this tenant subscription.")
    {
        TenantId = tenantId;
        FeatureKey = featureKey;
    }

    public Guid TenantId { get; }
    public string FeatureKey { get; }
}
