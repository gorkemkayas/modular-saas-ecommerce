using BuildingBlocks.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Infrastructure.Extensions.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestContext(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestContextMiddleware>();
        }
    }
}
