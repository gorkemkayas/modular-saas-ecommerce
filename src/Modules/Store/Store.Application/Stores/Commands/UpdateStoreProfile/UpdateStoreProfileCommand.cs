using MediatR;

namespace Store.Application.Stores.Commands.UpdateStoreProfile
{
    public sealed record UpdateStoreProfileCommand(Guid TenantId, string Name, string? Description, string? LogoUrl) : IRequest;
}
