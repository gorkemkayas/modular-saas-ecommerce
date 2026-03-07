using BuildingBlocks.Application.Abstractions.Authentication;
using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Authentication;
using BuildingBlocks.Infrastructure.Tenancy;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Extensions.Authentication
{
    public static class RequestContextExtensions
    {
        public static IServiceCollection AddRequestContexts(this IServiceCollection services)
        {
            services.AddScoped<UserRequestContext>();
            services.AddScoped<TenantRequestContext>();

            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<ITenantContext, TenantContext>();

            return services;
        }
    }
}
