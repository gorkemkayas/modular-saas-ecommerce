using Catalog.Application.Common.Models;
using Catalog.Application.Products.DTOs;

namespace Catalog.Application.Abstractions.Queries
{
    public interface IProductReadService
    {
        Task<ProductDto?> GetByIdAsync(Guid storeId, Guid productId, CancellationToken cancellationToken = default);
        Task<ProductDto?> GetBySlugAsync(Guid storeId, string slug, CancellationToken cancellationToken = default);
        Task<PagedResult<ProductSummaryDto>> SearchAsync(ProductSearchCriteria criteria, CancellationToken cancellationToken = default);
    }
}
