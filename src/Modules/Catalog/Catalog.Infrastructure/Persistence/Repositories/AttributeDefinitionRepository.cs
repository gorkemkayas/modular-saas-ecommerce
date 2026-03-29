using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    public sealed class AttributeDefinitionRepository : IAttributeDefinitionRepository
    {
        private readonly CatalogDbContext _context;

        public AttributeDefinitionRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AttributeDefinition attributeDefinition, CancellationToken cancellationToken = default)
        {
            await _context.AttributeDefinitions.AddAsync(attributeDefinition, cancellationToken);
        }

        public Task<AttributeDefinition?> GetByIdAsync(
            Guid storeId,
            Guid attributeDefinitionId,
            CancellationToken cancellationToken = default)
        {
            return _context.AttributeDefinitions.FirstOrDefaultAsync(
                x => x.StoreId == storeId && x.Id == attributeDefinitionId,
                cancellationToken);
        }

        public async Task<IReadOnlyCollection<AttributeDefinition>> GetByIdsAsync(
            Guid storeId,
            IReadOnlyCollection<Guid> attributeDefinitionIds,
            CancellationToken cancellationToken = default)
        {
            if (attributeDefinitionIds.Count == 0)
                return Array.Empty<AttributeDefinition>();

            return await _context.AttributeDefinitions
                .Where(x => x.StoreId == storeId && attributeDefinitionIds.Contains(x.Id))
                .ToArrayAsync(cancellationToken);
        }

        public Task<bool> ExistsByCodeAsync(
            Guid storeId,
            AttributeCode code,
            Guid? excludedAttributeDefinitionId = null,
            CancellationToken cancellationToken = default)
        {
            return _context.AttributeDefinitions.AnyAsync(
                x => x.StoreId == storeId
                    && x.Code == code
                    && (!excludedAttributeDefinitionId.HasValue || x.Id != excludedAttributeDefinitionId.Value),
                cancellationToken);
        }
    }
}
