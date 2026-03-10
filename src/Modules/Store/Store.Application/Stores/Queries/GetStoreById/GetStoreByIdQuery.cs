using MediatR;
using Store.Application.DTOs;

namespace Store.Application.Stores.Queries.GetStoreById
{
    public sealed record GetStoreByIdQuery(Guid StoreId) : IRequest<StoreDto?>;
}

