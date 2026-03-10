using MediatR;

namespace Store.Application.Stores.Commands.SuspendStore
{
    public sealed record SuspendStoreCommand(Guid TenantId) : IRequest;
}
