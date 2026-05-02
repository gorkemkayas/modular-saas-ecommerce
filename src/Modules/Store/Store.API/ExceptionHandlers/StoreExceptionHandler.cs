using BuildingBlocks.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Store.Application.Exceptions;
using Store.Domain.Exceptions;
using System.Diagnostics;

namespace Store.API.ExceptionHandlers
{
    public sealed class StoreExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<StoreExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public StoreExceptionHandler(
            ILogger<StoreExceptionHandler> logger,
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
            // Sadece Store modülüne ait exception'ları handle et
            if (!IsStoreException(exception))
                return false;

            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            _logger.LogError(
                exception,
                "[Store Module] Exception occurred: {Message} | TraceId: {TraceId} | Path: {Path}",
                exception.Message,
                traceId,
                httpContext.Request.Path);

            var (statusCode, title, type) = MapException(exception);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = GetDetail(exception),
                Instance = httpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] = traceId,
                    ["timestamp"] = DateTime.UtcNow,
                    ["module"] = "Store"
                }
            };

            // Development ortamında ek bilgiler
            if (_environment.IsDevelopment())
            {
                problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            // Custom exception property'lerini ekle
            AddCustomExceptionProperties(problemDetails, exception);

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static bool IsStoreException(Exception exception)
        {
            return exception is DomainException or Application.Exceptions.ApplicationException;
        }

        private static (int StatusCode, string Title, string Type) MapException(Exception exception)
        {
            return exception switch
            {
                // Domain Exceptions (400 - Bad Request)
                InvalidTenantException => (
                    StatusCodes.Status400BadRequest,
                    "Invalid Tenant",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.1"
                ),

                InvalidStoreNameException => (
                    StatusCodes.Status400BadRequest,
                    "Invalid Store Name",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.1"
                ),

                ArchivedStoreException => (
                    StatusCodes.Status400BadRequest,
                    "Archived Store Operation Not Allowed",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.1"
                ),

                InvalidStoreStatusException => (
                    StatusCodes.Status400BadRequest,
                    "Invalid Store Status",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.1"
                ),

                // Domain Exceptions (409 - Conflict)
                DuplicateSlugException => (
                    StatusCodes.Status409Conflict,
                    "Duplicate Slug",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.9"
                ),

                // Application Exceptions (404 - Not Found)
                StoreNotFoundException => (
                    StatusCodes.Status404NotFound,
                    "Store Not Found",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.5"
                ),

                StoreNotFoundByIdException => (
                    StatusCodes.Status404NotFound,
                    "Store Not Found",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.5"
                ),

                StoreNotFoundBySlugException => (
                    StatusCodes.Status404NotFound,
                    "Store Not Found",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.5"
                ),

                // Application Exceptions (409 - Conflict)
                DuplicateStoreSlugException => (
                    StatusCodes.Status409Conflict,
                    "Duplicate Store Slug",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.9"
                ),

                StoreAlreadyExistsForTenantException => (
                    StatusCodes.Status409Conflict,
                    "Store Already Exists",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.9"
                ),

                // Application Exceptions (403 - Forbidden)
                UnauthorizedStoreAccessException => (
                    StatusCodes.Status403Forbidden,
                    "Unauthorized Access",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.4"
                ),

                // Generic Domain Exception
                DomainException => (
                    StatusCodes.Status400BadRequest,
                    "Domain Rule Violation",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.1"
                ),

                // Generic Application Exception
                Application.Exceptions.ApplicationException => (
                    StatusCodes.Status400BadRequest,
                    "Application Error",
                    "https://tools.ietf.org/html/rfc9110#section-15.5.1"
                ),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "https://tools.ietf.org/html/rfc9110#section-15.6.1"
                )
            };
        }

        private string GetDetail(Exception exception)
        {
            if (!_environment.IsDevelopment())
            {
                return exception switch
                {
                    DomainException or Application.Exceptions.ApplicationException => exception.Message,
                    _ => "An error occurred while processing your request."
                };
            }

            return exception.Message;
        }

        private static void AddCustomExceptionProperties(ProblemDetails problemDetails, Exception exception)
        {
            switch (exception)
            {
                case StoreNotFoundException notFound:
                    problemDetails.Extensions["tenantId"] = notFound.TenantId;
                    break;

                case StoreNotFoundByIdException notFoundById:
                    problemDetails.Extensions["storeId"] = notFoundById.StoreId;
                    break;

                case StoreNotFoundBySlugException notFoundBySlug:
                    problemDetails.Extensions["slug"] = notFoundBySlug.Slug;
                    break;

                case DuplicateStoreSlugException duplicateSlug:
                    problemDetails.Extensions["slug"] = duplicateSlug.Slug;
                    break;

                case StoreAlreadyExistsForTenantException existingStore:
                    problemDetails.Extensions["tenantId"] = existingStore.TenantId;
                    break;

                case UnauthorizedStoreAccessException unauthorized:
                    problemDetails.Extensions["tenantId"] = unauthorized.TenantId;
                    if (unauthorized.StoreId.HasValue)
                    {
                        problemDetails.Extensions["storeId"] = unauthorized.StoreId.Value;
                    }
                    break;
            }
        }
    }
}
