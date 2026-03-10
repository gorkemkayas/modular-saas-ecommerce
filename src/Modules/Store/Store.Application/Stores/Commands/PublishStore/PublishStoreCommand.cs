using MediatR;

namespace Store.Application.Stores.Commands.PublishStore
{
    public sealed record PublishStoreCommand(Guid TenantId) : IRequest;
}
