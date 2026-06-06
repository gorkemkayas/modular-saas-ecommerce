namespace ECommerce.API.Contracts.Auth;

public sealed record RegisterCustomerResponse(
    Guid TenantUserId,
    Guid CustomerId,
    bool RequiresEmailVerification);
