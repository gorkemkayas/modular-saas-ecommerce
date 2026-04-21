using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Pricing.Domain.Exceptions;
using System.Diagnostics;
using ApplicationException = Pricing.Application.Exceptions.ApplicationException;

namespace ECommerce.API.ExceptionHandlers;

public sealed class PricingExceptionHandler : IExceptionHandler
{
    private readonly ILogger<PricingExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public PricingExceptionHandler(
        ILogger<PricingExceptionHandler> logger,
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
        if (exception is not PricingDomainException and not ApplicationException)
            return false;

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "[Pricing Module] {ExceptionType}: {Message} | TraceId: {TraceId} | Path: {Path} | TenantId: {TenantId}",
            exception.GetType().Name,
            exception.Message,
            traceId,
            httpContext.Request.Path,
            httpContext.User.FindFirst("tenantId")?.Value ?? "Unknown");

        var (statusCode, title) = exception switch
        {
            Pricing.Application.Exceptions.PriceListNotFoundException => (StatusCodes.Status404NotFound, "Price List Not Found"),
            Pricing.Application.Exceptions.DuplicateDefaultPriceListException => (StatusCodes.Status409Conflict, "Duplicate Default Price List"),
            Pricing.Application.Exceptions.InvalidPriceTargetException => (StatusCodes.Status400BadRequest, "Invalid Price Target"),
            Pricing.Application.Exceptions.PricingValidationException => (StatusCodes.Status400BadRequest, "Pricing Validation Error"),
            PricingDomainException => (StatusCodes.Status400BadRequest, "Pricing Domain Rule Violation"),
            ApplicationException => (StatusCodes.Status400BadRequest, "Pricing Application Error"),
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
                ["module"] = "Pricing"
            }
        };

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        switch (exception)
        {
            case Pricing.Application.Exceptions.PriceListNotFoundException notFound:
                problemDetails.Extensions["priceListId"] = notFound.PriceListId;
                break;

            case Pricing.Application.Exceptions.DuplicateDefaultPriceListException duplicate:
                problemDetails.Extensions["storeId"] = duplicate.StoreId;
                problemDetails.Extensions["currencyCode"] = duplicate.CurrencyCode;
                break;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
