using MediatR;
using Store.Application.DTOs;

namespace Store.Application.Stores.Queries.GetStoreByTenantId
{
    public sealed record GetStoreByTenantIdQuery(Guid TenantId) : IRequest<StoreDto?>;
}
