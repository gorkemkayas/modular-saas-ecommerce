namespace ECommerce.API.Integrations.Auth;

public sealed class AuthServiceOptions
{
    public const string SectionName = "AuthService";

    public string BaseUrl { get; init; } = string.Empty;
    public string RegisterPath { get; init; } = "/api/v1/auth/register";
    public string LoginPath { get; init; } = "/api/v1/auth/login";
    public string ApiKey { get; init; } = string.Empty;
}
