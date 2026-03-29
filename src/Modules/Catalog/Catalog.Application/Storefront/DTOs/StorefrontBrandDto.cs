namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontBrandDto(
        Guid Id,
        string Name,
        string Slug,
        string? Description,
        int ProductCount);
}
