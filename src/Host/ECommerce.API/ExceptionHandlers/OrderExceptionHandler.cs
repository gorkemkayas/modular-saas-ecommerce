using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Order.Domain.Exceptions;
using System.Diagnostics;
using ApplicationException = Order.Application.Exceptions.ApplicationException;

namespace ECommerce.API.ExceptionHandlers;

public sealed class OrderExceptionHandler : IExceptionHandler
{
    private readonly ILogger<OrderExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public OrderExceptionHandler(
        ILogger<OrderExceptionHandler> logger,
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
        if (exception is not OrderDomainException and not ApplicationException)
            return false;

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "[Order Module] {ExceptionType}: {Message} | TraceId: {TraceId} | Path: {Path} | TenantId: {TenantId}",
            exception.GetType().Name,
            exception.Message,
            traceId,
            httpContext.Request.Path,
            httpContext.User.FindFirst("tenantId")?.Value ?? "Unknown");

        var (statusCode, title) = exception switch
        {
            Order.Application.Exceptions.OrderNotFoundException => (StatusCodes.Status404NotFound, "Order Not Found"),
            Order.Application.Exceptions.OrderValidationException => (StatusCodes.Status400BadRequest, "Order Validation Error"),
            Order.Application.Exceptions.OrderPricingUnavailableException => (StatusCodes.Status400BadRequest, "Order Pricing Unavailable"),
            Order.Application.Exceptions.OrderCatalogItemUnavailableException => (StatusCodes.Status400BadRequest, "Order Item Unavailable"),
            Order.Application.Exceptions.OrderInventoryUnavailableException => (StatusCodes.Status409Conflict, "Order Inventory Unavailable"),
            Order.Application.Exceptions.UnauthorizedOrderAccessException => (StatusCodes.Status403Forbidden, "Unauthorized Order Access"),
            OrderDomainException => (StatusCodes.Status400BadRequest, "Order Domain Rule Violation"),
            ApplicationException => (StatusCodes.Status400BadRequest, "Order Application Error"),
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
                ["module"] = "Order"
            }
        };

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        switch (exception)
        {
            case Order.Application.Exceptions.OrderNotFoundException notFound:
                problemDetails.Extensions["orderId"] = notFound.OrderId;
                break;

            case Order.Application.Exceptions.OrderPricingUnavailableException pricing:
                problemDetails.Extensions["productId"] = pricing.ProductId;
                problemDetails.Extensions["productVariantId"] = pricing.ProductVariantId;
                problemDetails.Extensions["currencyCode"] = pricing.CurrencyCode;
                break;

            case Order.Application.Exceptions.OrderCatalogItemUnavailableException catalog:
                problemDetails.Extensions["productId"] = catalog.ProductId;
                problemDetails.Extensions["productVariantId"] = catalog.ProductVariantId;
                break;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
