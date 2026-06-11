using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Subscription.Domain.Exceptions;
using System.Diagnostics;
using ApplicationException = Subscription.Application.Exceptions.ApplicationException;

namespace ECommerce.API.ExceptionHandlers;

public sealed class SubscriptionExceptionHandler : IExceptionHandler
{
    private readonly ILogger<SubscriptionExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public SubscriptionExceptionHandler(
        ILogger<SubscriptionExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not SubscriptionDomainException and not ApplicationException)
            return false;

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "[Subscription Module] {ExceptionType}: {Message} | TraceId: {TraceId} | Path: {Path} | TenantId: {TenantId}",
            exception.GetType().Name,
            exception.Message,
            traceId,
            httpContext.Request.Path,
            httpContext.User.FindFirst("tenantId")?.Value ?? "Unknown");

        var (statusCode, title) = exception switch
        {
            Subscription.Application.Exceptions.PlanNotFoundException => (StatusCodes.Status404NotFound, "Subscription Plan Not Found"),
            Subscription.Application.Exceptions.SubscriptionValidationException => (StatusCodes.Status400BadRequest, "Subscription Validation Error"),
            SubscriptionDomainException => (StatusCodes.Status400BadRequest, "Subscription Domain Rule Violation"),
            ApplicationException => (StatusCodes.Status400BadRequest, "Subscription Application Error"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{statusCode}",
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId,
                ["timestamp"] = DateTime.UtcNow,
                ["module"] = "Subscription"
            }
        };

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        if (exception is Subscription.Application.Exceptions.PlanNotFoundException planNotFound)
            problemDetails.Extensions["planCode"] = planNotFound.PlanCode;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
