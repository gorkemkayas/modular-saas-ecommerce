namespace Notification.Application.Notifications.Services;

public interface IStoreBrandingProvider
{
    Task<StoreBrandingInfo?> GetAsync(
        Guid storeId,
        CancellationToken cancellationToken = default);
}

public sealed record StoreBrandingInfo(
    string Name,
    string? LogoUrl);
