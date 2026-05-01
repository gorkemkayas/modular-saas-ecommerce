using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Notification.Domain.Exceptions;
using System.Diagnostics;
using ApplicationException = Notification.Application.Exceptions.ApplicationException;

namespace ECommerce.API.ExceptionHandlers;

public sealed class NotificationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotificationExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public NotificationExceptionHandler(
        ILogger<NotificationExceptionHandler> logger,
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
        if (exception is not NotificationDomainException and not ApplicationException)
            return false;

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "[Notification Module] {ExceptionType}: {Message} | TraceId: {TraceId} | Path: {Path}",
            exception.GetType().Name,
            exception.Message,
            traceId,
            httpContext.Request.Path);

        var (statusCode, title) = exception switch
        {
            Notification.Application.Exceptions.NotificationTemplateNotFoundException => (StatusCodes.Status404NotFound, "Notification Template Not Found"),
            Notification.Application.Exceptions.NotificationDispatchNotFoundException => (StatusCodes.Status404NotFound, "Notification Dispatch Not Found"),
            Notification.Application.Exceptions.NotificationTemplateAlreadyExistsException => (StatusCodes.Status409Conflict, "Notification Template Already Exists"),
            Notification.Application.Exceptions.NotificationTemplateValidationException => (StatusCodes.Status400BadRequest, "Notification Template Validation Error"),
            Notification.Application.Exceptions.NotificationWebhookValidationException => (StatusCodes.Status400BadRequest, "Notification Webhook Validation Error"),
            NotificationDomainException => (StatusCodes.Status400BadRequest, "Notification Domain Rule Violation"),
            ApplicationException => (StatusCodes.Status400BadRequest, "Notification Application Error"),
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
                ["module"] = "Notification"
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
