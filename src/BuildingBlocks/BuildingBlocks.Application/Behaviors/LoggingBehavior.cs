using BuildingBlocks.Application.Abstractions.Authentication;
using BuildingBlocks.Application.Abstractions.Tenancy;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BuildingBlocks.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUser currentUser,
        ITenantContext tenantContext)
    {
        _logger = logger;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        // Contextual bilgileri ekle
        var userId = _currentUser.UserId?.ToString() ?? "Anonymous";
        var tenantId = _tenantContext.TenantId?.ToString() ?? "Unknown";

        _logger.LogInformation(
            "Handling {RequestName} | User: {UserId} | Tenant: {TenantId}",
            requestName,
            userId,
            tenantId);

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestName} successfully in {ElapsedMilliseconds}ms | User: {UserId} | Tenant: {TenantId}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                userId,
                tenantId);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms | User: {UserId} | Tenant: {TenantId} | Error: {ErrorMessage}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                userId,
                tenantId,
                ex.Message);

            throw;
        }
    }
}