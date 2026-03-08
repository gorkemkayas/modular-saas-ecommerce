using Store.Application.DTOs;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Queries.GetStoreBySlug
{
    public sealed class GetStoreBySlugQueryHandler
    {
        private readonly IStoreRepository _storeRepository;

        public GetStoreBySlugQueryHandler(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        public async Task<StoreDto?> Handle(
            GetStoreBySlugQuery query,
            CancellationToken cancellationToken = default)
        {
            var slug = Slug.Create(query.Slug);

            var store = await _storeRepository.GetBySlugAsync(slug, cancellationToken);

            if (store is null)
                return null;

            return new StoreDto(
                store.Id,
                store.TenantId,
                store.Name,
                store.Slug.Value,
                store.Description,
                store.LogoUrl,
                store.Status,
                store.IsPublished);
        }
    }
}
