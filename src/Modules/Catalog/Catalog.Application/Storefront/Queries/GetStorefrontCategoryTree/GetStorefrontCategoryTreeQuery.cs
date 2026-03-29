using Catalog.Application.Storefront.DTOs;
using MediatR;

namespace Catalog.Application.Storefront.Queries.GetStorefrontCategoryTree
{
    public sealed record GetStorefrontCategoryTreeQuery(Guid StoreId)
        : IRequest<IReadOnlyCollection<StorefrontCategoryTreeNodeDto>>;
}
