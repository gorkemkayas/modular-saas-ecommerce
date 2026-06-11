using MediatR;
using Store.Application.DTOs;

namespace Store.Application.Stores.Queries.ListPublishedStorefrontStores
{
    public sealed record ListPublishedStorefrontStoresQuery(int Limit = 16)
        : IRequest<IReadOnlyCollection<StorefrontStoreSummaryDto>>;
}
