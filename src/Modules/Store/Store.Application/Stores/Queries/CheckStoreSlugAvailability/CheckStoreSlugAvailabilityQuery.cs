using MediatR;

namespace Store.Application.Stores.Queries.CheckStoreSlugAvailability
{
    public sealed record CheckStoreSlugAvailabilityQuery(string Slug) : IRequest<bool>;
}
