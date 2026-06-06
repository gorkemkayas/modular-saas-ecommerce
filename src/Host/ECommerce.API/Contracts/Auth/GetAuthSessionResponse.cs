namespace ECommerce.API.Contracts.Auth;

public sealed record GetAuthSessionResponse(
    bool IsAuthenticated,
    Guid? ExternalUserId,
    string? Email,
    string? Name,
    int? TenantId,
    bool CanAccessAdmin,
    string? StoreSlug);
