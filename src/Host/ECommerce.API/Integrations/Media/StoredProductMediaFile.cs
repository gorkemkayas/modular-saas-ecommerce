using Catalog.Domain.Enums;

namespace ECommerce.API.Integrations.Media;

public sealed record StoredProductMediaFile(
    string Url,
    MediaType MediaType,
    string OriginalFileName)
{
    public bool IsImage => MediaType == MediaType.Image;
    public bool IsVideo => MediaType == MediaType.Video;
}
