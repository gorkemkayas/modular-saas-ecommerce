using MediatR;

namespace Catalog.Application.Brands.Commands.CreateBrand
{
    public sealed record CreateBrandCommand(
        Guid StoreId,
        string Name,
        string Slug,
        string? Description) : IRequest<Guid>;
}
