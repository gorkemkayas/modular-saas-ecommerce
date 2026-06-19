using MediatR;

namespace Store.Application.Stores.Commands.DeletePendingStore;

public sealed record DeletePendingStoreCommand(Guid TenantId) : IRequest;
