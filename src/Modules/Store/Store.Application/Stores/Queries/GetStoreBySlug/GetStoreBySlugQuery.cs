using MediatR;
using Store.Application.DTOs;

namespace Store.Application.Stores.Queries.GetStoreBySlug
{
    public sealed record GetStoreBySlugQuery(string Slug) : IRequest<StoreDto?>;
}
