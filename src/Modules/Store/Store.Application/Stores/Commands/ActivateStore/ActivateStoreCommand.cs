using MediatR;

namespace Store.Application.Stores.Commands.ActivateStore
{
    public sealed record ActivateStoreCommand(Guid TenantId) : IRequest;
}
