namespace Store.Application.Exceptions
{
    public sealed class StoreAlreadyExistsForTenantException : ApplicationException
    {
        public Guid TenantId { get; }

        public StoreAlreadyExistsForTenantException(Guid tenantId)
            : base($"A store already exists for tenant '{tenantId}'.")
        {
            TenantId = tenantId;
        }
    }
}
