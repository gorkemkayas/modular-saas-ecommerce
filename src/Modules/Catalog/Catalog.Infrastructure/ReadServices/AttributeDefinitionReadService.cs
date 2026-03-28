using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Attributes.DTOs;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.ReadServices
{
    public sealed class AttributeDefinitionReadService : IAttributeDefinitionReadService
    {
        private readonly CatalogDbContext _context;

        public AttributeDefinitionReadService(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<AttributeDefinitionDto?> GetByIdAsync(
            Guid storeId,
            Guid attributeDefinitionId,
            CancellationToken cancellationToken = default)
        {
            var attributeDefinition = await _context.AttributeDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == attributeDefinitionId, cancellationToken);

            return attributeDefinition == null
                ? null
                : new AttributeDefinitionDto(
                    attributeDefinition.Id,
                    attributeDefinition.StoreId,
                    attributeDefinition.Name,
                    attributeDefinition.Code.Value,
                    attributeDefinition.DataType,
                    attributeDefinition.IsRequired,
                    attributeDefinition.IsFilterable,
                    attributeDefinition.IsVariantDefining,
                    attributeDefinition.IsActive,
                    attributeDefinition.CreatedAtUtc,
                    attributeDefinition.UpdatedAtUtc);
        }

        public async Task<IReadOnlyCollection<AttributeDefinitionDto>> ListByStoreAsync(
            Guid storeId,
            bool activeOnly,
            CancellationToken cancellationToken = default)
        {
            var query = _context.AttributeDefinitions
                .AsNoTracking()
                .Where(x => x.StoreId == storeId);

            if (activeOnly)
                query = query.Where(x => x.IsActive);

            var attributeDefinitions = await query
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            return attributeDefinitions
                .Select(x => new AttributeDefinitionDto(
                    x.Id,
                    x.StoreId,
                    x.Name,
                    x.Code.Value,
                    x.DataType,
                    x.IsRequired,
                    x.IsFilterable,
                    x.IsVariantDefining,
                    x.IsActive,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc))
                .ToArray();
        }
    }
}
