namespace ECommerce.API.Options;

public sealed class AppDataProtectionOptions
{
    public const string SectionName = "DataProtection";

    public string ApplicationName { get; init; } = "ECommerce.API";

    public string KeysPath { get; init; } = string.Empty;
}
