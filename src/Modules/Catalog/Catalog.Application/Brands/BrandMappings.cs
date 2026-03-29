using Catalog.Application.Brands.DTOs;
using Catalog.Domain.Entities;

namespace Catalog.Application.Brands
{
    internal static class BrandMappings
    {
        public static BrandDto ToDto(this Brand brand)
        {
            return new BrandDto(
                brand.Id,
                brand.StoreId,
                brand.Name,
                brand.Slug.Value,
                brand.Description,
                brand.IsActive,
                brand.CreatedAtUtc,
                brand.UpdatedAtUtc);
        }
    }
}
