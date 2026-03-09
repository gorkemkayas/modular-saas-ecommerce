namespace Store.Application.Stores.Commands.ChangeStoreSlug
{
    public sealed record ChangeStoreSlugCommand(Guid TenantId, string NewSlug);
}
