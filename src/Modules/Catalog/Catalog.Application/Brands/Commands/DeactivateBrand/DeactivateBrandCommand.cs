using MediatR;

namespace Catalog.Application.Brands.Commands.DeactivateBrand
{
    public sealed record DeactivateBrandCommand(Guid StoreId, Guid BrandId) : IRequest;
}
