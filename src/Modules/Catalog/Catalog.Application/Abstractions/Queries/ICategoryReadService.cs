using Catalog.Application.Categories.DTOs;

namespace Catalog.Application.Abstractions.Queries
{
    public interface ICategoryReadService
    {
        Task<CategoryDto?> GetByIdAsync(Guid storeId, Guid categoryId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<CategoryTreeNodeDto>> GetTreeAsync(Guid storeId, CancellationToken cancellationToken = default);
    }
}
