using Inventory.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ApplicationException = Inventory.Application.Exceptions.ApplicationException;

namespace ECommerce.API.ExceptionHandlers;

public sealed class InventoryExceptionHandler : IExceptionHandler
{
    private readonly ILogger<InventoryExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public InventoryExceptionHandler(
        ILogger<InventoryExceptionHandler> logger,
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
        if (exception is not InventoryDomainException and not ApplicationException)
            return false;

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "[Inventory Module] {ExceptionType}: {Message} | TraceId: {TraceId} | Path: {Path} | TenantId: {TenantId}",
            exception.GetType().Name,
            exception.Message,
            traceId,
            httpContext.Request.Path,
            httpContext.User.FindFirst("tenantId")?.Value ?? "Unknown");

        var (statusCode, title) = exception switch
        {
            Inventory.Application.Exceptions.InventoryItemNotFoundException => (StatusCodes.Status404NotFound, "Inventory Item Not Found"),
            Inventory.Application.Exceptions.InventoryReservationNotFoundException => (StatusCodes.Status404NotFound, "Inventory Reservation Not Found"),
            Inventory.Application.Exceptions.DuplicateInventoryItemException => (StatusCodes.Status409Conflict, "Duplicate Inventory Item"),
            Inventory.Application.Exceptions.InventoryInsufficientStockException => (StatusCodes.Status409Conflict, "Inventory Insufficient Stock"),
            Inventory.Application.Exceptions.InventoryValidationException => (StatusCodes.Status400BadRequest, "Inventory Validation Error"),
            InventoryDomainException => (StatusCodes.Status400BadRequest, "Inventory Domain Rule Violation"),
            ApplicationException => (StatusCodes.Status400BadRequest, "Inventory Application Error"),
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
                ["module"] = "Inventory"
            }
        };

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
