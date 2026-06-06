using ECommerce.API.Integrations.Media;
using Microsoft.Extensions.Options;

namespace ECommerce.API.Extensions;

public static class MediaStorageRegistration
{
    public static IServiceCollection AddMediaStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CloudinaryMediaStorageOptions>(
            configuration.GetSection(CloudinaryMediaStorageOptions.SectionName));

        services.AddHttpClient<IProductMediaStorageService, CloudinaryProductMediaStorageService>(
            (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<CloudinaryMediaStorageOptions>>().Value;
                var apiBaseUrl = string.IsNullOrWhiteSpace(options.ApiBaseUrl)
                    ? "https://api.cloudinary.com"
                    : options.ApiBaseUrl.Trim();

                client.BaseAddress = new Uri(
                    apiBaseUrl.EndsWith('/') ? apiBaseUrl : $"{apiBaseUrl}/",
                    UriKind.Absolute);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("modular-saas-ecommerce/1.0");
            });

        return services;
    }
}
