using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ECommerce.API.Integrations.Auth;

public sealed class AuthServiceClient : IAuthServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly AuthServiceOptions _options;
    private readonly ILogger<AuthServiceClient> _logger;

    public AuthServiceClient(
        HttpClient httpClient,
        IOptions<AuthServiceOptions> options,
        ILogger<AuthServiceClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AuthServiceRegisterOutcome> RegisterAsync(
        AuthServiceRegisterCommand command,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            NormalizeRelativePath(_options.RegisterPath, "api/v1/auth/register"));

        ApplyAuthorizationHeader(httpRequest);

        httpRequest.Content = JsonContent.Create(
            new AuthServiceRegisterRequest(
                command.FirstName,
                command.LastName,
                command.Email,
                command.Password,
                command.TenantId,
                false),
            options: JsonOptions);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var payload = TryExtractRegisterResponse(responseContent);

            if (payload is null || payload.TenantUserId == Guid.Empty)
            {
                _logger.LogError(
                    "Auth service returned an invalid register response for tenant {TenantId}. Response: {ResponseContent}",
                    command.TenantId,
                    string.IsNullOrWhiteSpace(responseContent) ? "<empty>" : responseContent);

                return new AuthServiceRegisterOutcome(
                    false,
                    null,
                    false,
                    StatusCodes.Status502BadGateway,
                    "Auth service returned an invalid registration response.");
            }

            return new AuthServiceRegisterOutcome(
                true,
                payload.TenantUserId,
                payload.RequiresEmailVerification,
                (int)response.StatusCode,
                null);
        }

        var errorMessage = TryExtractErrorMessage(responseContent)
            ?? "Auth service registration failed.";

        _logger.LogWarning(
            "Auth service registration failed | TenantId: {TenantId} | StatusCode: {StatusCode} | Error: {Error}",
            command.TenantId,
            (int)response.StatusCode,
            errorMessage);

        return new AuthServiceRegisterOutcome(
            false,
            null,
            false,
            (int)response.StatusCode,
            errorMessage);
    }

    public async Task<AuthServiceLoginOutcome> LoginAsync(
        AuthServiceLoginCommand command,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            NormalizeRelativePath(_options.LoginPath, "api/v1/auth/login"));

        ApplyAuthorizationHeader(httpRequest);

        httpRequest.Content = JsonContent.Create(
            new AuthServiceLoginRequest(
                command.Email,
                command.Password,
                command.IsPersistent),
            options: JsonOptions);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var payload = TryExtractLoginResponse(responseContent);
            var refreshToken = payload?.RefreshToken ?? TryExtractRefreshTokenFromSetCookie(response);

            if (payload is null || string.IsNullOrWhiteSpace(payload.Token))
            {
                _logger.LogError(
                    "Auth service returned an invalid login response. Response: {ResponseContent}",
                    string.IsNullOrWhiteSpace(responseContent) ? "<empty>" : responseContent);

                return new AuthServiceLoginOutcome(
                    false,
                    null,
                    null,
                    StatusCodes.Status502BadGateway,
                    "Auth service returned an invalid login response.");
            }

            return new AuthServiceLoginOutcome(
                true,
                payload.Token,
                refreshToken,
                (int)response.StatusCode,
                null);
        }

        var errorMessage = TryExtractErrorMessage(responseContent)
            ?? "Auth service login failed.";

        _logger.LogWarning(
            "Auth service login failed | StatusCode: {StatusCode} | Error: {Error}",
            (int)response.StatusCode,
            errorMessage);

        return new AuthServiceLoginOutcome(
            false,
            null,
            null,
            (int)response.StatusCode,
            errorMessage);
    }

    public async Task<AuthServiceRefreshOutcome> RefreshAsync(
        AuthServiceRefreshCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return new AuthServiceRefreshOutcome(
                false,
                null,
                null,
                StatusCodes.Status400BadRequest,
                "Refresh token is required.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            NormalizeRelativePath(_options.RefreshPath, "api/v1/auth/refresh"));

        ApplyAuthorizationHeader(httpRequest);

        httpRequest.Content = JsonContent.Create(
            new AuthServiceRefreshRequest(command.RefreshToken),
            options: JsonOptions);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var payload = TryExtractLoginResponse(responseContent);
            var refreshToken = payload?.RefreshToken ?? TryExtractRefreshTokenFromSetCookie(response);

            if (payload is null || string.IsNullOrWhiteSpace(payload.Token))
            {
                _logger.LogError(
                    "Auth service returned an invalid refresh response. Response: {ResponseContent}",
                    string.IsNullOrWhiteSpace(responseContent) ? "<empty>" : responseContent);

                return new AuthServiceRefreshOutcome(
                    false,
                    null,
                    null,
                    StatusCodes.Status502BadGateway,
                    "Auth service returned an invalid refresh response.");
            }

            return new AuthServiceRefreshOutcome(
                true,
                payload.Token,
                refreshToken,
                (int)response.StatusCode,
                null);
        }

        var errorMessage = TryExtractErrorMessage(responseContent)
            ?? "Auth service refresh failed.";

        _logger.LogWarning(
            "Auth service refresh failed | StatusCode: {StatusCode} | Error: {Error}",
            (int)response.StatusCode,
            errorMessage);

        return new AuthServiceRefreshOutcome(
            false,
            null,
            null,
            (int)response.StatusCode,
            errorMessage);
    }

    private void ApplyAuthorizationHeader(HttpRequestMessage httpRequest)
    {
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _options.ApiKey.Trim());
        }
    }

    private static AuthServiceRegisterResponse? TryExtractRegisterResponse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (TryReadRegisterResponse(root, out var directResponse))
                return directResponse;

            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("data", out var dataElement))
            {
                if (TryReadRegisterResponse(dataElement, out var wrappedResponse))
                    return wrappedResponse;

                if (TryReadGuid(dataElement, out var tenantUserId))
                {
                    var requiresEmailVerification = TryReadBoolean(root, "requiresEmailVerification")
                        ?? false;

                    return new AuthServiceRegisterResponse(
                        tenantUserId,
                        requiresEmailVerification);
                }
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    private static AuthServiceLoginResponse? TryExtractLoginResponse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (TryReadLoginResponse(root, out var directResponse))
                return directResponse;

            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("data", out var dataElement) &&
                TryReadLoginResponse(dataElement, out var wrappedResponse))
            {
                return wrappedResponse;
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    private static string NormalizeRelativePath(string? path, string fallbackPath)
    {
        return string.IsNullOrWhiteSpace(path)
            ? fallbackPath
            : path.TrimStart('/');
    }

    private static string? TryExtractErrorMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (TryGetString(root, "detail", out var detail))
                return detail;

            if (TryGetString(root, "message", out var message))
                return message;

            if (TryGetString(root, "title", out var title))
                return title;

            if (TryGetString(root, "error", out var error))
                return error;
        }
        catch (JsonException)
        {
        }

        return content.Trim();
    }

    private static bool TryReadRegisterResponse(
        JsonElement element,
        out AuthServiceRegisterResponse? response)
    {
        response = null;

        if (TryReadGuid(element, "tenantUserId", out var tenantUserId) ||
            TryReadGuid(element, "id", out tenantUserId) ||
            TryReadGuid(element, "userId", out tenantUserId))
        {
            var requiresEmailVerification = TryReadBoolean(element, "requiresEmailVerification")
                ?? false;

            response = new AuthServiceRegisterResponse(
                tenantUserId,
                requiresEmailVerification);

            return true;
        }

        return false;
    }

    private static bool TryReadLoginResponse(
        JsonElement element,
        out AuthServiceLoginResponse? response)
    {
        response = null;

        if (!TryGetString(element, "token", out var token) || string.IsNullOrWhiteSpace(token))
            return false;

        TryGetString(element, "refreshToken", out var refreshToken);
        response = new AuthServiceLoginResponse(token, refreshToken);
        return true;
    }

    private static bool TryReadGuid(
        JsonElement element,
        string propertyName,
        out Guid value)
    {
        value = Guid.Empty;

        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        return TryReadGuid(property, out value);
    }

    private static bool TryReadGuid(JsonElement element, out Guid value)
    {
        value = Guid.Empty;

        return element.ValueKind == JsonValueKind.String &&
               Guid.TryParse(element.GetString(), out value);
    }

    private static bool? TryReadBoolean(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static string? TryExtractRefreshTokenFromSetCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
            return null;

        foreach (var setCookieValue in values)
        {
            var segment = setCookieValue.Split(';', 2)[0];
            var separatorIndex = segment.IndexOf('=', StringComparison.Ordinal);
            if (separatorIndex <= 0 || separatorIndex == segment.Length - 1)
                continue;

            var name = segment[..separatorIndex].Trim();
            var value = segment[(separatorIndex + 1)..].Trim();

            if (name.Contains("refresh", StringComparison.OrdinalIgnoreCase))
                return value;
        }

        return null;
    }

    private static bool TryGetString(JsonElement root, string propertyName, out string? value)
    {
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String)
        {
            value = property.GetString();
            return !string.IsNullOrWhiteSpace(value);
        }

        value = null;
        return false;
    }

    private sealed record AuthServiceRegisterRequest(
        string Name,
        string Surname,
        string Email,
        string Password,
        int TenantId,
        bool IsPersistent);

    private sealed record AuthServiceRefreshRequest(
        string RefreshToken);

    private sealed record AuthServiceRegisterResponse(
        Guid TenantUserId,
        bool RequiresEmailVerification);

    private sealed record AuthServiceLoginRequest(
        string Email,
        string Password,
        bool IsPersistent);

    private sealed record AuthServiceLoginResponse(
        string Token,
        string? RefreshToken);
}
