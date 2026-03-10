using MediatR;
using Store.Application.DTOs;

namespace Store.Application.Stores.Queries.SuggestAvailableSlug
{
    public sealed record SuggestAvailableSlugQuery(string Name) : IRequest<SlugSuggestionDto>;
}
