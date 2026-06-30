using Notification.Application.Notifications.Services;
using Store.Contracts;

namespace Notification.Infrastructure.Services;

public sealed class StoreBrandingProvider : IStoreBrandingProvider
{
    private readonly IStoreModuleApi _storeModuleApi;

    public StoreBrandingProvider(IStoreModuleApi storeModuleApi)
    {
        _storeModuleApi = storeModuleApi;
    }

    public async Task<StoreBrandingInfo?> GetAsync(
        Guid storeId,
        CancellationToken cancellationToken = default)
    {
        var branding = await _storeModuleApi.GetBrandingAsync(storeId, cancellationToken);

        return branding is null
            ? null
            : new StoreBrandingInfo(branding.Name, branding.LogoUrl);
    }
}
