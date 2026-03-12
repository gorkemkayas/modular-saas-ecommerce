namespace Store.Application.Exceptions
{
    public sealed class UnauthorizedStoreAccessException : ApplicationException
    {
        public Guid TenantId { get; }
        public Guid? StoreId { get; }

        public UnauthorizedStoreAccessException(Guid tenantId)
            : base($"Unauthorized access to store for Tenant ID {tenantId}.")
        {
            TenantId = tenantId;
        }

        public UnauthorizedStoreAccessException(Guid tenantId, Guid storeId)
            : base($"Unauthorized access to store {storeId} for Tenant ID {tenantId}.")
        {
            TenantId = tenantId;
            StoreId = storeId;
        }
    }
}
