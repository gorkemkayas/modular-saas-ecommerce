using ECommerce.API.Options;
using Microsoft.AspNetCore.DataProtection;

namespace ECommerce.API.Extensions;

public static class DataProtectionRegistration
{
    public static IServiceCollection AddConfiguredDataProtection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AppDataProtectionOptions>(
            configuration.GetSection(AppDataProtectionOptions.SectionName));

        var options = configuration
            .GetSection(AppDataProtectionOptions.SectionName)
            .Get<AppDataProtectionOptions>() ?? new AppDataProtectionOptions();

        var builder = services.AddDataProtection()
            .SetApplicationName(string.IsNullOrWhiteSpace(options.ApplicationName)
                ? "ECommerce.API"
                : options.ApplicationName.Trim());

        if (!string.IsNullOrWhiteSpace(options.KeysPath))
        {
            var fullPath = Path.GetFullPath(options.KeysPath.Trim());
            Directory.CreateDirectory(fullPath);
            builder.PersistKeysToFileSystem(new DirectoryInfo(fullPath));
        }

        return services;
    }
}
