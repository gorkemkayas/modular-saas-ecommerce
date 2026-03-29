using Catalog.Application.Attributes.DTOs;

namespace Catalog.Application.Abstractions.Queries
{
    public interface IAttributeDefinitionReadService
    {
        Task<AttributeDefinitionDto?> GetByIdAsync(Guid storeId, Guid attributeDefinitionId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<AttributeDefinitionDto>> ListByStoreAsync(Guid storeId, bool activeOnly, CancellationToken cancellationToken = default);
    }
}
