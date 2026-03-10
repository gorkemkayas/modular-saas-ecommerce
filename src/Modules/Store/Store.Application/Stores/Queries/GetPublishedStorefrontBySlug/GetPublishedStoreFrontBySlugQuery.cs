using MediatR;
using Store.Application.DTOs;

namespace Store.Application.Stores.Queries.GetPublishedStorefrontBySlug
{
    public sealed record GetPublishedStoreFrontBySlugQuery(string Slug) : IRequest<StorefrontDto?>;
}
