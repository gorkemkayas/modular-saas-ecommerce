using Store.Application.DTOs;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Queries.GetPublishedStorefrontBySlug
{
    public sealed class GetPublishedStoreFrontBySlugQueryHandler
    {
        private readonly IStoreRepository _storeRepository;

        public GetPublishedStoreFrontBySlugQueryHandler(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        public async Task<StorefrontDto?> Handle(GetPublishedStoreFrontBySlugQuery query, CancellationToken cancellationToken)
        {
            var slug = Slug.Create(query.Slug);
            var store = await _storeRepository.GetBySlugAsync(slug,cancellationToken);
            if(store is null)
                return null;
            if(!store.IsPublished || store.Status != StoreStatus.Active)
                return null;

            return new StorefrontDto(
                store.TenantId,
                store.Name,
                store.Slug.Value,
                store.Description,
                store.LogoUrl
            );
        }
    }
}
