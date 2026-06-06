namespace ECommerce.API.Integrations.Auth;

public sealed record AuthServiceLoginCommand(
    string Email,
    string Password,
    bool IsPersistent);

public sealed record AuthServiceLoginOutcome(
    bool IsSuccess,
    string? Token,
    string? RefreshToken,
    int? StatusCode,
    string? ErrorMessage);
