using ECommerce.API.Integrations.Auth;
using Microsoft.Extensions.Options;

namespace ECommerce.API.Extensions;

public static class AuthServiceRegistration
{
    public static IServiceCollection AddAuthServiceIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthServiceOptions>(
            configuration.GetSection(AuthServiceOptions.SectionName));

        services.AddHttpClient<IAuthServiceClient, AuthServiceClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<AuthServiceOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
                throw new InvalidOperationException("Auth service base URL is missing.");

            var baseUrl = options.BaseUrl.Trim();
            client.BaseAddress = new Uri(
                baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/",
                UriKind.Absolute);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("modular-saas-ecommerce/1.0");
        });

        return services;
    }
}
