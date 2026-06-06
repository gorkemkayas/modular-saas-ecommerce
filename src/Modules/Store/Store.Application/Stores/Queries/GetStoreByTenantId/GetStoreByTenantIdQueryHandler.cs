using MediatR;
using Store.Application.DTOs;
using Store.Domain.Stores;

namespace Store.Application.Stores.Queries.GetStoreByTenantId
{
    public sealed class GetStoreByTenantIdQueryHandler : IRequestHandler<GetStoreByTenantIdQuery, StoreDto?>
    {
        private readonly IStoreRepository _storeRepository;

        public GetStoreByTenantIdQueryHandler(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        public async Task<StoreDto?> Handle(
            GetStoreByTenantIdQuery query,
            CancellationToken cancellationToken = default)
        {
            var store = await _storeRepository.GetByTenantIdAsync(query.TenantId, cancellationToken);

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
                store.IsPublished,
                store.HeroImageUrl,
                store.HeroMediaType,
                store.HeroEyebrowText,
                store.HeroTitle,
                store.HeroAccentTitle,
                store.HeroDescription,
                store.HeroPrimaryButtonText,
                store.LoginPageImageUrl,
                store.RegisterPageImageUrl);
        }
    }
}
