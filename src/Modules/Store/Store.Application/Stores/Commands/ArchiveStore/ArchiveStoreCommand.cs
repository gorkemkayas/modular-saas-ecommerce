using MediatR;

namespace Store.Application.Stores.Commands.ArchiveStore
{
    public sealed record ArchiveStoreCommand(Guid TenantId) : IRequest;
}
