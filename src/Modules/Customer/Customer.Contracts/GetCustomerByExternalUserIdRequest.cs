namespace Customer.Contracts;

public sealed record GetCustomerByExternalUserIdRequest(
    Guid TenantId,
    Guid ExternalUserId);
