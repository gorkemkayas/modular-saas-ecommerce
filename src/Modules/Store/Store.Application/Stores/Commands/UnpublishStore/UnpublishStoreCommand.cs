using MediatR;

namespace Store.Application.Stores.Commands.UnpublishStore
{
    public sealed record UnpublishStoreCommand(Guid TenantId) : IRequest;
}
