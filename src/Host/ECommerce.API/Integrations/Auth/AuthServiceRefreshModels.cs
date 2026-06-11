namespace ECommerce.API.Integrations.Auth;

public sealed record AuthServiceRefreshCommand(
    string RefreshToken);

public sealed record AuthServiceRefreshOutcome(
    bool IsSuccess,
    string? Token,
    string? RefreshToken,
    int? StatusCode,
    string? ErrorMessage);
