namespace ECommerce.API.Integrations.Auth;

public sealed record AuthServiceRegisterCommand(
    int TenantId,
    string Email,
    string Password,
    string FirstName,
    string LastName);

public sealed record AuthServiceRegisterOutcome(
    bool IsSuccess,
    Guid? TenantUserId,
    bool RequiresEmailVerification,
    int? StatusCode,
    string? ErrorMessage);
