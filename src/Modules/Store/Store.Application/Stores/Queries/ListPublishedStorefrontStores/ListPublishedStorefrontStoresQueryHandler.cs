using MediatR;
using Store.Application.DTOs;
using Store.Domain.Stores;

namespace Store.Application.Stores.Queries.ListPublishedStorefrontStores
{
    public sealed class ListPublishedStorefrontStoresQueryHandler
        : IRequestHandler<ListPublishedStorefrontStoresQuery, IReadOnlyCollection<StorefrontStoreSummaryDto>>
    {
        private const int DefaultLimit = 16;
        private const int MaxLimit = 32;

        private readonly IStoreRepository _storeRepository;

        public ListPublishedStorefrontStoresQueryHandler(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        public async Task<IReadOnlyCollection<StorefrontStoreSummaryDto>> Handle(
            ListPublishedStorefrontStoresQuery query,
            CancellationToken cancellationToken)
        {
            var limit = query.Limit <= 0
                ? DefaultLimit
                : Math.Min(query.Limit, MaxLimit);

            var stores = await _storeRepository.ListPublishedAsync(limit, cancellationToken);

            return stores
                .Select(store => new StorefrontStoreSummaryDto(
                    store.TenantId,
                    store.Name,
                    store.Slug.Value,
                    store.LogoUrl))
                .ToList();
        }
    }
}
