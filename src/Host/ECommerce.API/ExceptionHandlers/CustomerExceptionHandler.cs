using Customer.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ApplicationException = Customer.Application.Exceptions.ApplicationException;

namespace ECommerce.API.ExceptionHandlers;

public sealed class CustomerExceptionHandler : IExceptionHandler
{
    private readonly ILogger<CustomerExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public CustomerExceptionHandler(
        ILogger<CustomerExceptionHandler> logger,
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
        if (exception is not CustomerDomainException and not ApplicationException)
            return false;

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "[Customer Module] {ExceptionType}: {Message} | TraceId: {TraceId} | Path: {Path} | TenantId: {TenantId}",
            exception.GetType().Name,
            exception.Message,
            traceId,
            httpContext.Request.Path,
            httpContext.User.FindFirst("tenantId")?.Value ?? "Unknown");

        var (statusCode, title) = exception switch
        {
            Customer.Application.Exceptions.CustomerNotFoundException => (StatusCodes.Status404NotFound, "Customer Not Found"),
            CustomerDomainException => (StatusCodes.Status400BadRequest, "Customer Domain Rule Violation"),
            ApplicationException => (StatusCodes.Status400BadRequest, "Customer Application Error"),
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
                ["module"] = "Customer"
            }
        };

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        if (exception is Customer.Application.Exceptions.CustomerNotFoundException notFound)
        {
            problemDetails.Extensions["tenantId"] = notFound.TenantId;

            if (notFound.CustomerId.HasValue)
                problemDetails.Extensions["customerId"] = notFound.CustomerId.Value;

            if (notFound.ExternalUserId.HasValue)
                problemDetails.Extensions["externalUserId"] = notFound.ExternalUserId.Value;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
