namespace Store.Application.DTOs
{
    public sealed record StorefrontDto(
        Guid TenantId,
        string Name,
        string Slug,
        string? Description,
        string? LogoUrl
);
}
