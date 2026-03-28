using MediatR;

namespace Catalog.Application.Brands.Commands.ActivateBrand
{
    public sealed record ActivateBrandCommand(Guid StoreId, Guid BrandId) : IRequest;
}
