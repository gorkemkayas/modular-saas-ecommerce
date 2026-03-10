using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Application.Extensions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
namespace BuildingBlocks.Infrastructure.Tenancy
{
    public sealed class TenantContext : ITenantContext
    {
        private readonly TenantRequestContext _tenantRequestContext;

        public TenantContext(TenantRequestContext tenantRequestContext)
        {
            _tenantRequestContext = tenantRequestContext;
        }

        public int? TenantId => _tenantRequestContext.TenantId;

        public Guid? TenantIdAsGuid => TenantId.HasValue
            ? TenantIdConverter.ToGuid(TenantId.Value)
            : null;

        public bool HasTenant => TenantId.HasValue;
    }
}
