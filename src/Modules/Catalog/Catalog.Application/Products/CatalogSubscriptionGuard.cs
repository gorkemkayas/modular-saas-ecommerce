using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using Subscription.Contracts;

namespace Catalog.Application.Products;

internal static class CatalogSubscriptionGuard
{
    public static async Task EnsureCanCreateProductAsync(
        Guid storeId,
        IProductRepository productRepository,
        ISubscriptionModuleApi subscriptionModuleApi,
        CancellationToken cancellationToken)
    {
        var currentCount = await productRepository.CountNonArchivedByStoreIdAsync(
            storeId,
            cancellationToken);

        await EnsureQuotaHasCapacityAsync(
            storeId,
            SubscriptionQuotaKeys.CatalogProducts,
            currentCount,
            subscriptionModuleApi,
            cancellationToken);
    }

    public static Task EnsureCanCreateCategoryAsync(
        Guid storeId,
        int currentCount,
        ISubscriptionModuleApi subscriptionModuleApi,
        CancellationToken cancellationToken)
    {
        return EnsureQuotaHasCapacityAsync(
            storeId,
            SubscriptionQuotaKeys.CatalogCategories,
            currentCount,
            subscriptionModuleApi,
            cancellationToken);
    }

    public static Task EnsureCanAddProductMediaAsync(
        Guid storeId,
        int currentCount,
        ISubscriptionModuleApi subscriptionModuleApi,
        CancellationToken cancellationToken)
    {
        return EnsureQuotaHasCapacityAsync(
            storeId,
            SubscriptionQuotaKeys.CatalogMediaPerProduct,
            currentCount,
            subscriptionModuleApi,
            cancellationToken);
    }

    public static async Task EnsureCanCreateVariantProductAsync(
        Guid storeId,
        ISubscriptionModuleApi subscriptionModuleApi,
        CancellationToken cancellationToken)
    {
        var hasFeature = await subscriptionModuleApi.HasFeatureAsync(
            new FeatureAccessRequest(storeId, SubscriptionFeatureKeys.VariantProducts),
            cancellationToken);

        if (!hasFeature)
            throw new CatalogFeatureUnavailableException(storeId, SubscriptionFeatureKeys.VariantProducts);
    }

    private static async Task EnsureQuotaHasCapacityAsync(
        Guid storeId,
        string quotaKey,
        int currentCount,
        ISubscriptionModuleApi subscriptionModuleApi,
        CancellationToken cancellationToken)
    {
        var quota = await subscriptionModuleApi.GetQuotaAsync(
            new QuotaRequest(storeId, quotaKey),
            cancellationToken);

        if (quota is null)
            throw new CatalogValidationException($"Catalog quota '{quotaKey}' is not configured for this tenant.");

        if (!quota.Limit.HasValue)
            return;

        if (currentCount >= quota.Limit.Value)
            throw new CatalogQuotaExceededException(storeId, quotaKey, currentCount, quota.Limit.Value);
    }
}
