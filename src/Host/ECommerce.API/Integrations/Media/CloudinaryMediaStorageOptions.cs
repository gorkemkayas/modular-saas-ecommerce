namespace ECommerce.API.Integrations.Media;

public sealed class CloudinaryMediaStorageOptions
{
    public const string SectionName = "CloudinaryMediaStorage";

    public string ApiBaseUrl { get; set; } = "https://api.cloudinary.com";
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string Folder { get; set; } = "stores/product-media";
}
