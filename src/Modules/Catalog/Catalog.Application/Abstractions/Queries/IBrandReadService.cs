using Catalog.Application.Brands.DTOs;

namespace Catalog.Application.Abstractions.Queries
{
    public interface IBrandReadService
    {
        Task<BrandDto?> GetByIdAsync(Guid storeId, Guid brandId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<BrandDto>> SearchAsync(Guid storeId, string? searchTerm, bool activeOnly, CancellationToken cancellationToken = default);
    }
}
