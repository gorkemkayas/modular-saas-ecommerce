namespace Customer.Application.Exceptions;

public sealed class CustomerNotFoundException : ApplicationException
{
    public Guid TenantId { get; }
    public Guid? CustomerId { get; }
    public Guid? ExternalUserId { get; }

    public CustomerNotFoundException(Guid tenantId, Guid customerId)
        : base($"Customer {customerId} was not found for tenant {tenantId}.")
    {
        TenantId = tenantId;
        CustomerId = customerId;
    }

    public CustomerNotFoundException(Guid tenantId, Guid externalUserId, bool byExternalUserId)
        : base($"Customer with external user {externalUserId} was not found for tenant {tenantId}.")
    {
        TenantId = tenantId;
        ExternalUserId = externalUserId;
    }
}
