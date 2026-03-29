using Catalog.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ApplicationException = Catalog.Application.Exceptions.ApplicationException;

namespace ECommerce.API.ExceptionHandlers
{
    public sealed class CatalogExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<CatalogExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public CatalogExceptionHandler(
            ILogger<CatalogExceptionHandler> logger,
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
            if (!IsCatalogException(exception))
                return false;

            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            _logger.LogError(
                exception,
                "[Catalog Module] {ExceptionType}: {Message} | TraceId: {TraceId} | Path: {Path}",
                exception.GetType().Name,
                exception.Message,
                traceId,
                httpContext.Request.Path);

            var (statusCode, title) = MapException(exception);

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
                    ["module"] = "Catalog"
                }
            };

            if (_environment.IsDevelopment())
            {
                problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            AddCustomProperties(problemDetails, exception);

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static bool IsCatalogException(Exception exception)
        {
            return exception is CatalogDomainException or ApplicationException;
        }

        private static (int StatusCode, string Title) MapException(Exception exception)
        {
            return exception switch
            {
                DuplicateSkuException => (StatusCodes.Status409Conflict, "Duplicate SKU"),
                DuplicateVariantCombinationException => (StatusCodes.Status409Conflict, "Duplicate Variant Combination"),
                Catalog.Application.Exceptions.DuplicateProductSlugException => (StatusCodes.Status409Conflict, "Duplicate Product Slug"),
                Catalog.Application.Exceptions.DuplicateProductSkuException => (StatusCodes.Status409Conflict, "Duplicate Product SKU"),
                Catalog.Application.Exceptions.DuplicateCategorySlugException => (StatusCodes.Status409Conflict, "Duplicate Category Slug"),
                Catalog.Application.Exceptions.DuplicateBrandSlugException => (StatusCodes.Status409Conflict, "Duplicate Brand Slug"),
                Catalog.Application.Exceptions.DuplicateAttributeCodeException => (StatusCodes.Status409Conflict, "Duplicate Attribute Code"),

                Catalog.Application.Exceptions.ProductNotFoundException => (StatusCodes.Status404NotFound, "Product Not Found"),
                Catalog.Application.Exceptions.CategoryNotFoundException => (StatusCodes.Status404NotFound, "Category Not Found"),
                Catalog.Application.Exceptions.BrandNotFoundException => (StatusCodes.Status404NotFound, "Brand Not Found"),
                Catalog.Application.Exceptions.AttributeDefinitionNotFoundException => (StatusCodes.Status404NotFound, "Attribute Definition Not Found"),

                CatalogDomainException => (StatusCodes.Status400BadRequest, "Catalog Domain Rule Violation"),
                ApplicationException => (StatusCodes.Status400BadRequest, "Catalog Application Error"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };
        }

        private static void AddCustomProperties(ProblemDetails problemDetails, Exception exception)
        {
            switch (exception)
            {
                case Catalog.Application.Exceptions.ProductNotFoundException notFound:
                    problemDetails.Extensions["productId"] = notFound.ProductId;
                    break;

                case Catalog.Application.Exceptions.CategoryNotFoundException notFound:
                    problemDetails.Extensions["categoryId"] = notFound.CategoryId;
                    break;

                case Catalog.Application.Exceptions.BrandNotFoundException notFound:
                    problemDetails.Extensions["brandId"] = notFound.BrandId;
                    break;

                case Catalog.Application.Exceptions.AttributeDefinitionNotFoundException notFound:
                    problemDetails.Extensions["attributeDefinitionId"] = notFound.AttributeDefinitionId;
                    break;

                case Catalog.Application.Exceptions.DuplicateProductSlugException duplicate:
                    problemDetails.Extensions["slug"] = duplicate.Slug;
                    break;

                case Catalog.Application.Exceptions.DuplicateProductSkuException duplicate:
                    problemDetails.Extensions["sku"] = duplicate.Sku;
                    break;

                case Catalog.Application.Exceptions.DuplicateCategorySlugException duplicate:
                    problemDetails.Extensions["slug"] = duplicate.Slug;
                    break;

                case Catalog.Application.Exceptions.DuplicateBrandSlugException duplicate:
                    problemDetails.Extensions["slug"] = duplicate.Slug;
                    break;

                case Catalog.Application.Exceptions.DuplicateAttributeCodeException duplicate:
                    problemDetails.Extensions["code"] = duplicate.Code;
                    break;
            }
        }
    }
}
