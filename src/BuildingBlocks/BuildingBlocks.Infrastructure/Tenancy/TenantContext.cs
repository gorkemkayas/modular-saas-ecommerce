using BuildingBlocks.Application.Abstractions.Tenancy;
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

        public bool HasTenant => TenantId.HasValue;
    }
}
