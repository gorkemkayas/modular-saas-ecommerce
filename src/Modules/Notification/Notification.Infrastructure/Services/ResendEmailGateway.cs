using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.Notifications.Services;
using Notification.Infrastructure.Options;

namespace Notification.Infrastructure.Services;

public sealed class ResendEmailGateway : IEmailGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly NotificationEmailOptions _options;
    private readonly ILogger<ResendEmailGateway> _logger;

    public ResendEmailGateway(
        HttpClient httpClient,
        IOptions<NotificationEmailOptions> options,
        ILogger<ResendEmailGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public string ProviderName => "Resend";

    public async Task<EmailSendResult> SendAsync(
        EmailSendRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new EmailSendResult(
                false,
                null,
                null,
                "configuration_error",
                "Resend API key is missing.");
        }

        if (string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            return new EmailSendResult(
                false,
                null,
                null,
                "configuration_error",
                "Resend sender email is missing.");
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "emails");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var payload = new ResendSendEmailRequest(
            ComposeFrom(),
            new[] { request.ToEmail },
            request.Subject,
            BuildHtmlBody(request.Body),
            request.Body,
            string.IsNullOrWhiteSpace(_options.ReplyTo) ? null : new[] { _options.ReplyTo! });

        httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var success = JsonSerializer.Deserialize<ResendSendEmailResponse>(responseContent, JsonOptions);

            return new EmailSendResult(
                true,
                success?.Id,
                success?.Id,
                null,
                null);
        }

        var error = TryDeserializeError(responseContent);

        _logger.LogWarning(
            "Resend email send failed | StatusCode: {StatusCode} | ErrorCode: {ErrorCode} | ErrorMessage: {ErrorMessage}",
            (int)response.StatusCode,
            error?.Name,
            error?.Message);

        return new EmailSendResult(
            false,
            null,
            null,
            error?.Name ?? $"http_{(int)response.StatusCode}",
            error?.Message ?? "Resend email send failed.");
    }

    private string ComposeFrom()
    {
        return string.IsNullOrWhiteSpace(_options.FromName)
            ? _options.FromEmail.Trim()
            : $"{_options.FromName.Trim()} <{_options.FromEmail.Trim()}>";
    }

    private static string BuildHtmlBody(string body)
    {
        var encoded = WebUtility.HtmlEncode(body ?? string.Empty)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\n", "<br />", StringComparison.Ordinal);

        return $"""
                <div style="font-family:Arial,Helvetica,sans-serif;white-space:normal;line-height:1.6;">
                  {encoded}
                </div>
                """;
    }

    private static ResendErrorResponse? TryDeserializeError(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ResendErrorResponse>(content, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private sealed record ResendSendEmailRequest(
        string From,
        string[] To,
        string Subject,
        string Html,
        string Text,
        string[]? ReplyTo);

    private sealed record ResendSendEmailResponse(string Id);

    private sealed record ResendErrorResponse(string Name, string Message);
}
