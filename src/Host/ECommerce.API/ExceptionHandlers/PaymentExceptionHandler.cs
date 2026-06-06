using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Payment.Domain.Exceptions;
using System.Diagnostics;
using ApplicationException = Payment.Application.Exceptions.ApplicationException;

namespace ECommerce.API.ExceptionHandlers;

public sealed class PaymentExceptionHandler : IExceptionHandler
{
    private readonly ILogger<PaymentExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public PaymentExceptionHandler(
        ILogger<PaymentExceptionHandler> logger,
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
        if (exception is not PaymentDomainException and not ApplicationException)
            return false;

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "[Payment Module] {ExceptionType}: {Message} | TraceId: {TraceId} | Path: {Path}",
            exception.GetType().Name,
            exception.Message,
            traceId,
            httpContext.Request.Path);

        var (statusCode, title) = exception switch
        {
            Payment.Application.Exceptions.PaymentNotFoundException => (StatusCodes.Status404NotFound, "Payment Not Found"),
            Payment.Application.Exceptions.PaymentValidationException => (StatusCodes.Status400BadRequest, "Payment Validation Error"),
            Payment.Application.Exceptions.UnauthorizedPaymentAccessException => (StatusCodes.Status403Forbidden, "Unauthorized Payment Access"),
            Payment.Application.Exceptions.PaymentWebhookValidationException => (StatusCodes.Status400BadRequest, "Payment Webhook Validation Error"),
            Payment.Application.Exceptions.PaymentProviderAccountNotConfiguredException => (StatusCodes.Status400BadRequest, "Payment Provider Account Not Configured"),
            PaymentDomainException => (StatusCodes.Status400BadRequest, "Payment Domain Rule Violation"),
            ApplicationException => (StatusCodes.Status400BadRequest, "Payment Application Error"),
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
                ["module"] = "Payment"
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
