using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Catalog.Domain.Enums;
using Microsoft.Extensions.Options;

namespace ECommerce.API.Integrations.Media;

public sealed class CloudinaryProductMediaStorageService : IProductMediaStorageService
{
    private readonly HttpClient _httpClient;
    private readonly CloudinaryMediaStorageOptions _options;

    public CloudinaryProductMediaStorageService(
        HttpClient httpClient,
        IOptions<CloudinaryMediaStorageOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<StoredProductMediaFile> UploadAsync(
        Guid storeId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        if (file.Length <= 0)
            throw new InvalidOperationException("The uploaded media file is empty.");

        var mediaType = ResolveMediaType(file);
        var resourceType = mediaType == MediaType.Video ? "video" : "image";
        var folder = BuildFolder(storeId);

        using var multipartContent = new MultipartFormDataContent();

        await using var fileStream = file.OpenReadStream();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType);

        multipartContent.Add(fileContent, "file", file.FileName);
        multipartContent.Add(new StringContent(folder), "folder");
        multipartContent.Add(new StringContent("true"), "use_filename");
        multipartContent.Add(new StringContent("true"), "unique_filename");
        multipartContent.Add(new StringContent("false"), "overwrite");

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/v1_1/{_options.CloudName.Trim()}/{resourceType}/upload")
        {
            Content = multipartContent
        };

        var credentials = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(
                $"{_options.ApiKey.Trim()}:{_options.ApiSecret.Trim()}"));

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Cloudinary upload failed with status {(int)response.StatusCode}: {payload}");
        }

        var result = await response.Content.ReadFromJsonAsync<CloudinaryUploadResponse>(
            cancellationToken: cancellationToken);

        if (result is null || string.IsNullOrWhiteSpace(result.SecureUrl))
            throw new InvalidOperationException("Cloudinary upload succeeded but no secure URL was returned.");

        return new StoredProductMediaFile(
            result.SecureUrl,
            mediaType,
            file.FileName);
    }

    private void EnsureConfigured()
    {
        if (IsMissingOrPlaceholder(_options.CloudName) ||
            IsMissingOrPlaceholder(_options.ApiKey) ||
            IsMissingOrPlaceholder(_options.ApiSecret))
        {
            throw new InvalidOperationException(
                "Cloudinary media storage is not configured. Set CloudinaryMediaStorage options first.");
        }
    }

    private static bool IsMissingOrPlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return value.Trim().Equals("SET_VIA_USER_SECRETS", StringComparison.OrdinalIgnoreCase);
    }

    private static MediaType ResolveMediaType(IFormFile file)
    {
        if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return MediaType.Image;

        if (file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return MediaType.Video;

        var extension = Path.GetExtension(file.FileName);

        if (IsImageExtension(extension))
            return MediaType.Image;

        if (IsVideoExtension(extension))
            return MediaType.Video;

        throw new InvalidOperationException("Only image and video uploads are supported for product media.");
    }

    private string BuildFolder(Guid storeId)
    {
        var normalizedFolder = (_options.Folder ?? string.Empty).Trim().Trim('/');

        return string.IsNullOrWhiteSpace(normalizedFolder)
            ? storeId.ToString("N")
            : $"{normalizedFolder}/{storeId:N}";
    }

    private static bool IsImageExtension(string? extension)
    {
        return extension?.ToLowerInvariant() is ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".avif";
    }

    private static bool IsVideoExtension(string? extension)
    {
        return extension?.ToLowerInvariant() is ".mp4" or ".webm" or ".mov" or ".m4v" or ".ogg";
    }

    private sealed class CloudinaryUploadResponse
    {
        [JsonPropertyName("secure_url")]
        public string SecureUrl { get; set; } = string.Empty;
    }
}
