namespace Store.Application.DTOs
{
    public sealed record StorefrontStoreSummaryDto(
        Guid TenantId,
        string Name,
        string Slug,
        string? LogoUrl);
}
