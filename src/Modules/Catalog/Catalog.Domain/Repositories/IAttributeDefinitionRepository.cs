using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Repositories
{
    public interface IAttributeDefinitionRepository
    {
        Task AddAsync(AttributeDefinition attributeDefinition, CancellationToken cancellationToken = default);
        Task<AttributeDefinition?> GetByIdAsync(Guid storeId, Guid attributeDefinitionId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<AttributeDefinition>> GetByIdsAsync(
            Guid storeId,
            IReadOnlyCollection<Guid> attributeDefinitionIds,
            CancellationToken cancellationToken = default);
        Task<bool> ExistsByCodeAsync(
            Guid storeId,
            AttributeCode code,
            Guid? excludedAttributeDefinitionId = null,
            CancellationToken cancellationToken = default);
    }
}
