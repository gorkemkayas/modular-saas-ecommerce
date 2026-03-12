namespace Store.Application.Exceptions
{
    public sealed class StoreNotFoundException : ApplicationException
    {
        public Guid TenantId { get; }

        public StoreNotFoundException(Guid tenantId)
            : base($"Store with Tenant ID {tenantId} not found.")
        {
            TenantId = tenantId;
        }

        public StoreNotFoundException(Guid tenantId, string identifier)
            : base($"Store with {identifier} {tenantId} not found.")
        {
            TenantId = tenantId;
        }
    }
}
