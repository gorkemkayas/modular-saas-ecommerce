using MediatR;

namespace Catalog.Application.Brands.Commands.UpdateBrand
{
    public sealed record UpdateBrandCommand(
        Guid StoreId,
        Guid BrandId,
        string Name,
        string Slug,
        string? Description) : IRequest;
}
