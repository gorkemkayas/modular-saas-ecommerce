namespace BuildingBlocks.Infrastructure.Extensions.Authorization
{
        public static class AppPolicies
        {
            public const string TenantAdmin = "TenantAdminPolicy";
            public const string SuperAdmin = "SuperAdminPolicy";
            public const string TenantOrSuperAdmin = "TenantOrSuperAdminPolicy";
        }
}
