using Serilog;

namespace ECommerce.API.Extensions;

public static class SerilogExtensions
{
    public static Serilog.ILogger CreateLogger(this IConfiguration configuration)
    {
        return new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .CreateLogger();
    }

    public static Serilog.ILogger CreateBootstrapLogger()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

        return configuration.CreateLogger();
    }

    public static IApplicationBuilder UseEnrichedSerilogRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                
                
                var tenantId = httpContext.User.Claims.FirstOrDefault(c => c.Type == "tenantId")?.Value;
                if (!string.IsNullOrEmpty(tenantId))
                {
                    diagnosticContext.Set("TenantId", tenantId);
                }
            };
        });

        return app;
    }
}