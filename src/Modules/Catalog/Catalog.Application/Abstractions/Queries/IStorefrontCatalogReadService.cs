using Catalog.Application.Common.Models;
using Catalog.Application.Storefront.DTOs;

namespace Catalog.Application.Abstractions.Queries
{
    public interface IStorefrontCatalogReadService
    {
        Task<PagedResult<StorefrontProductSummaryDto>> SearchProductsAsync(
            StorefrontProductSearchCriteria criteria,
            CancellationToken cancellationToken = default);

        Task<StorefrontProductDto?> GetProductBySlugAsync(
            Guid storeId,
            string slug,
            string currencyCode,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<StorefrontCategoryTreeNodeDto>> GetCategoryTreeAsync(
            Guid storeId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<StorefrontBrandDto>> SearchBrandsAsync(
            Guid storeId,
            string? searchTerm,
            CancellationToken cancellationToken = default);

        Task<StorefrontCatalogFacetsDto> GetFacetsAsync(
            StorefrontCatalogFacetCriteria criteria,
            CancellationToken cancellationToken = default);
    }
}
