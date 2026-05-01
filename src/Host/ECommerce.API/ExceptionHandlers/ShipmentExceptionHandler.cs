using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shipment.Domain.Exceptions;
using System.Diagnostics;
using ApplicationException = Shipment.Application.Exceptions.ApplicationException;

namespace ECommerce.API.ExceptionHandlers;

public sealed class ShipmentExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ShipmentExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public ShipmentExceptionHandler(
        ILogger<ShipmentExceptionHandler> logger,
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
        if (exception is not ShipmentDomainException and not ApplicationException)
            return false;

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "[Shipment Module] {ExceptionType}: {Message} | TraceId: {TraceId} | Path: {Path}",
            exception.GetType().Name,
            exception.Message,
            traceId,
            httpContext.Request.Path);

        var (statusCode, title) = exception switch
        {
            Shipment.Application.Exceptions.ShipmentNotFoundException => (StatusCodes.Status404NotFound, "Shipment Not Found"),
            Shipment.Application.Exceptions.ShipmentAlreadyExistsForOrderException => (StatusCodes.Status409Conflict, "Shipment Already Exists"),
            Shipment.Application.Exceptions.ShipmentCreationNotAllowedException => (StatusCodes.Status400BadRequest, "Shipment Creation Not Allowed"),
            Shipment.Application.Exceptions.ShipmentDispatchNotAllowedException => (StatusCodes.Status400BadRequest, "Shipment Dispatch Not Allowed"),
            Shipment.Application.Exceptions.ShipmentCancellationNotAllowedException => (StatusCodes.Status400BadRequest, "Shipment Cancellation Not Allowed"),
            Shipment.Application.Exceptions.ShipmentValidationException => (StatusCodes.Status400BadRequest, "Shipment Validation Error"),
            Shipment.Application.Exceptions.UnauthorizedShipmentAccessException => (StatusCodes.Status403Forbidden, "Unauthorized Shipment Access"),
            ShipmentDomainException => (StatusCodes.Status400BadRequest, "Shipment Domain Rule Violation"),
            ApplicationException => (StatusCodes.Status400BadRequest, "Shipment Application Error"),
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
                ["module"] = "Shipment"
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
