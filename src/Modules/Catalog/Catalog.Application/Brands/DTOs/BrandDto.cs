namespace Catalog.Application.Brands.DTOs
{
    public sealed record BrandDto(
        Guid Id,
        Guid StoreId,
        string Name,
        string Slug,
        string? Description,
        bool IsActive,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);
}
