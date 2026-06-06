using BuildingBlocks.Application.Extensions;
using BuildingBlocks.Infrastructure.Extensions.Authentication;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using Customer.Application.Customers.Commands.SyncCustomerFromIdentity;
using ECommerce.API.Contracts.Auth;
using ECommerce.API.Integrations.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Stores.Queries.GetPublishedStorefrontBySlug;
using Store.Application.Stores.Queries.GetStoreBySlug;
using Store.Application.Stores.Queries.GetStoreByTenantId;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ECommerce.API.Controllers.Customer;

[Route("api/auth")]
[ApiController]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IAuthServiceClient _authServiceClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ISender sender,
        IAuthServiceClient authServiceClient,
        ILogger<AuthController> logger)
    {
        _sender = sender;
        _authServiceClient = authServiceClient;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterCustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
            return ValidationProblem(new ValidationProblemDetails(validationErrors));

        try
        {
            var store = await _sender.Send(
                new GetPublishedStoreFrontBySlugQuery(request.StoreSlug.Trim()),
                cancellationToken);

            if (store is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Store not found",
                    Detail = "No published store was found for the provided store slug."
                });
            }

            var tenantId = TenantIdConverter.ToInt(store.TenantId);
            if (TenantIdConverter.ToGuid(tenantId) != store.TenantId)
            {
                _logger.LogError(
                    "Store tenant id could not be converted back to the auth service tenant id | StoreSlug: {StoreSlug} | TenantId: {TenantId}",
                    request.StoreSlug,
                    store.TenantId);

                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Tenant mapping error",
                    detail: "The resolved store tenant could not be mapped to the auth service tenant id.");
            }

            var result = await _authServiceClient.RegisterAsync(
                new AuthServiceRegisterCommand(
                    tenantId,
                    request.Email.Trim(),
                    request.Password,
                    request.FirstName.Trim(),
                    request.LastName.Trim()),
                cancellationToken);

            if (!result.IsSuccess)
            {
                var statusCode = result.StatusCode is >= 400 and < 500
                    ? result.StatusCode.Value
                    : StatusCodes.Status502BadGateway;

                return Problem(
                    statusCode: statusCode,
                    title: "Registration failed",
                    detail: result.ErrorMessage ?? "Registration could not be completed.");
            }

            Guid customerId;

            try
            {
                customerId = await _sender.Send(
                    new SyncCustomerFromIdentityCommand(
                        store.TenantId,
                        result.TenantUserId!.Value,
                        request.Email.Trim(),
                        request.FirstName.Trim(),
                        request.LastName.Trim()),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Auth service registration succeeded but customer sync failed | StoreSlug: {StoreSlug} | TenantId: {TenantId} | TenantUserId: {TenantUserId}",
                    request.StoreSlug,
                    store.TenantId,
                    result.TenantUserId);

                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "Customer sync failed",
                    detail: "The account was created in the authentication service, but customer provisioning failed in e-commerce.");
            }

            return Ok(new RegisterCustomerResponse(
                result.TenantUserId!.Value,
                customerId,
                result.RequiresEmailVerification));
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["storeSlug"] = [ex.Message]
            }));
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
            return ValidationProblem(new ValidationProblemDetails(validationErrors));

        try
        {
            var normalizedStoreSlug = request.StoreSlug.Trim();
            var publishedStore = !request.AllowInactiveStore
                ? await _sender.Send(
                    new GetPublishedStoreFrontBySlugQuery(normalizedStoreSlug),
                    cancellationToken)
                : null;
            var resolvedStore = publishedStore is null && request.AllowInactiveStore
                ? await _sender.Send(
                    new GetStoreBySlugQuery(normalizedStoreSlug),
                    cancellationToken)
                : null;

            if (publishedStore is null && resolvedStore is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Store not found",
                    Detail = request.AllowInactiveStore
                        ? "No store was found for the provided store slug."
                        : "No published store was found for the provided store slug."
                });
            }

            var storeTenantId = publishedStore?.TenantId ?? resolvedStore!.TenantId;
            var inactiveStore = resolvedStore is not null &&
                (!resolvedStore.IsPublished || resolvedStore.Status != global::Store.Domain.Stores.StoreStatus.Active);

            var expectedTenantId = TenantIdConverter.ToInt(storeTenantId);
            if (TenantIdConverter.ToGuid(expectedTenantId) != storeTenantId)
            {
                _logger.LogError(
                    "Store tenant id could not be converted back to the auth service tenant id | StoreSlug: {StoreSlug} | TenantId: {TenantId}",
                    request.StoreSlug,
                    storeTenantId);

                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Tenant mapping error",
                    detail: "The resolved store tenant could not be mapped to the auth service tenant id.");
            }

            var result = await _authServiceClient.LoginAsync(
                new AuthServiceLoginCommand(
                    request.Email.Trim(),
                    request.Password,
                    request.IsPersistent),
                cancellationToken);

            if (!result.IsSuccess)
            {
                var statusCode = result.StatusCode is >= 400 and < 500
                    ? result.StatusCode.Value
                    : StatusCodes.Status502BadGateway;

                return Problem(
                    statusCode: statusCode,
                    title: "Login failed",
                    detail: result.ErrorMessage ?? "Login could not be completed.");
            }

            var actualTenantId = TryReadTenantIdFromToken(result.Token!);
            if (actualTenantId is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "Invalid token response",
                    detail: "Auth service returned a token without a tenant id claim.");
            }

            if (actualTenantId.Value != expectedTenantId)
            {
                _logger.LogWarning(
                    "Auth service login returned a token for a different tenant | ExpectedTenantId: {ExpectedTenantId} | ActualTenantId: {ActualTenantId} | StoreSlug: {StoreSlug}",
                    expectedTenantId,
                    actualTenantId.Value,
                    request.StoreSlug);

                return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Tenant mismatch",
                    detail: "The authenticated user does not belong to the selected storefront.");
            }

            if (request.AllowInactiveStore && inactiveStore && !HasAdminAccess(result.Token!))
            {
                return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Admin access required",
                    detail: "Only store administrators can sign in while the storefront is inactive.");
            }

            if (!TryReadIdentityFromToken(result.Token!, out var externalUserId, out var email, out var firstName, out var lastName))
            {
                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "Invalid token response",
                    detail: "Auth service returned a token without the identity claims required for customer sync.");
            }

            try
            {
                await _sender.Send(
                    new SyncCustomerFromIdentityCommand(
                        storeTenantId,
                        externalUserId,
                        email,
                        firstName,
                        lastName),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Auth service login succeeded but customer sync failed | StoreSlug: {StoreSlug} | TenantId: {TenantId} | ExternalUserId: {ExternalUserId}",
                    request.StoreSlug,
                    storeTenantId,
                    externalUserId);

                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "Customer sync failed",
                    detail: "The user was authenticated, but the customer profile could not be provisioned in e-commerce.");
            }

            SetAuthCookies(result.Token!, result.RefreshToken, request.IsPersistent);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["storeSlug"] = [ex.Message]
            }));
        }
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        ClearAuthCookies();
        return NoContent();
    }

    [HttpGet("session")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetAuthSessionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSession(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Ok(new GetAuthSessionResponse(
                false,
                null,
                null,
                null,
                null,
                false,
                null));
        }

        Guid? externalUserId = Guid.TryParse(
            User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            out var parsedExternalUserId)
            ? parsedExternalUserId
            : null;

        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ??
                    User.FindFirstValue(ClaimTypes.Email);

        var name = User.FindFirstValue("name") ??
                   User.FindFirstValue(ClaimTypes.Name);

        var tenantId = TryReadTenantIdFromClaims(User);
        string? storeSlug = null;

        if (tenantId is not null)
        {
            var store = await _sender.Send(
                new GetStoreByTenantIdQuery(TenantIdConverter.ToGuid(tenantId.Value)),
                cancellationToken);

            storeSlug = store?.Slug;
        }

        return Ok(new GetAuthSessionResponse(
            true,
            externalUserId,
            email,
            name,
            tenantId,
            HasAdminAccess(User),
            storeSlug));
    }

    private static Dictionary<string, string[]> Validate(RegisterCustomerRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.StoreSlug))
            errors["storeSlug"] = ["Store slug is required."];

        if (string.IsNullOrWhiteSpace(request.Email))
            errors["email"] = ["Email is required."];

        if (string.IsNullOrWhiteSpace(request.Password))
            errors["password"] = ["Password is required."];

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors["firstName"] = ["First name is required."];

        if (string.IsNullOrWhiteSpace(request.LastName))
            errors["lastName"] = ["Last name is required."];

        return errors;
    }

    private static Dictionary<string, string[]> Validate(LoginCustomerRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.StoreSlug))
            errors["storeSlug"] = ["Store slug is required."];

        if (string.IsNullOrWhiteSpace(request.Email))
            errors["email"] = ["Email is required."];

        if (string.IsNullOrWhiteSpace(request.Password))
            errors["password"] = ["Password is required."];

        return errors;
    }

    private static int? TryReadTenantIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
            return null;

        var jwt = handler.ReadJwtToken(token);
        var tenantIdValue = jwt.Claims.FirstOrDefault(c => c.Type == "tenantId")?.Value;

        return int.TryParse(tenantIdValue, out var tenantId)
            ? tenantId
            : null;
    }

    private static int? TryReadTenantIdFromClaims(ClaimsPrincipal user)
    {
        var tenantIdValue = user.FindFirstValue("tenantId");

        return int.TryParse(tenantIdValue, out var tenantId)
            ? tenantId
            : null;
    }

    private static bool HasAdminAccess(ClaimsPrincipal user)
    {
        if (user.IsInRole(AppRoles.TenantAdmin) || user.IsInRole(AppRoles.SuperAdmin))
            return true;

        var roles = user.Claims
            .Where(c =>
                c.Type == ClaimTypes.Role ||
                c.Type == "role" ||
                c.Type == "roles" ||
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => c.Value)
            .ToArray();

        return roles.Any(role =>
            string.Equals(role, AppRoles.TenantAdmin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, AppRoles.SuperAdmin, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasAdminAccess(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
            return false;

        var jwt = handler.ReadJwtToken(token);
        var roles = jwt.Claims
            .Where(c =>
                c.Type == ClaimTypes.Role ||
                c.Type == "role" ||
                c.Type == "roles" ||
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => c.Value)
            .ToArray();

        return roles.Any(role =>
            string.Equals(role, AppRoles.TenantAdmin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, AppRoles.SuperAdmin, StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryReadIdentityFromToken(
        string token,
        out Guid externalUserId,
        out string email,
        out string firstName,
        out string lastName)
    {
        externalUserId = Guid.Empty;
        email = string.Empty;
        firstName = string.Empty;
        lastName = string.Empty;

        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
            return false;

        var jwt = handler.ReadJwtToken(token);
        var subject = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        email = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email)?.Value?.Trim() ?? string.Empty;

        if (!Guid.TryParse(subject, out externalUserId) || string.IsNullOrWhiteSpace(email))
            return false;

        firstName = jwt.Claims.FirstOrDefault(c => c.Type == "given_name" || c.Type == ClaimTypes.GivenName)?.Value?.Trim() ?? string.Empty;
        lastName = jwt.Claims.FirstOrDefault(c => c.Type == "family_name" || c.Type == ClaimTypes.Surname)?.Value?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            return true;

        var fullName = jwt.Claims.FirstOrDefault(c => c.Type == "name" || c.Type == ClaimTypes.Name)?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            firstName = !string.IsNullOrWhiteSpace(firstName) ? firstName : email.Split('@')[0];
            lastName = string.IsNullOrWhiteSpace(lastName) ? string.Empty : lastName;
            return !string.IsNullOrWhiteSpace(firstName);
        }

        var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (nameParts.Length == 0)
        {
            firstName = email.Split('@')[0];
            lastName = string.Empty;
            return true;
        }

        firstName = !string.IsNullOrWhiteSpace(firstName) ? firstName : nameParts[0];
        lastName = !string.IsNullOrWhiteSpace(lastName)
            ? lastName
            : nameParts.Length > 1
                ? string.Join(' ', nameParts.Skip(1))
                : string.Empty;

        return !string.IsNullOrWhiteSpace(firstName);
    }

    private void SetAuthCookies(string accessToken, string? refreshToken, bool isPersistent)
    {
        var accessCookieOptions = CreateCookieOptions(isPersistent);
        Response.Cookies.Append(AuthCookieNames.AccessToken, accessToken, accessCookieOptions);

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var refreshCookieOptions = CreateCookieOptions(isPersistent);
            Response.Cookies.Append(AuthCookieNames.RefreshToken, refreshToken, refreshCookieOptions);
        }
    }

    private void ClearAuthCookies()
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };

        Response.Cookies.Delete(AuthCookieNames.AccessToken, options);
        Response.Cookies.Delete(AuthCookieNames.RefreshToken, options);
    }

    private static CookieOptions CreateCookieOptions(bool isPersistent)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };

        if (isPersistent)
            options.Expires = DateTimeOffset.UtcNow.AddDays(30);

        return options;
    }
}
