using MediatR;

namespace Store.Application.Stores.Commands.ProvisionStoreForTenant
{
    public sealed record ProvisionStoreForTenantCommand(
    Guid TenantId,
    string Name,
    string Slug) : IRequest<Guid>;
}
