using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        if (request.Sender is null
            ? string.IsNullOrWhiteSpace(_options.FromEmail)
            : string.IsNullOrWhiteSpace(request.Sender.From))
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
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            httpRequest.Headers.Add("Idempotency-Key", request.IdempotencyKey);

        string? replyTo = request.Sender is null ? _options.ReplyTo : request.Sender.ReplyTo;

        var payload = new ResendSendEmailRequest(
            request.Sender?.From ?? ComposeFrom(),
            new[] { request.ToEmail },
            request.Subject,
            request.HtmlBody,
            request.Body,
            string.IsNullOrWhiteSpace(replyTo) ? null : new[] { replyTo });

        httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            ResendSendEmailResponse? success;
            try
            {
                success = JsonSerializer.Deserialize<ResendSendEmailResponse>(responseContent, JsonOptions);
            }
            catch (JsonException)
            {
                return new EmailSendResult(false, null, null,
                    "invalid_provider_response", "Resend returned an invalid success response.", IsTransientFailure: true);
            }
            if (string.IsNullOrWhiteSpace(success?.Id))
                return new EmailSendResult(false, null, null,
                    "invalid_provider_response", "Resend returned no message id.", IsTransientFailure: true);

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
            error?.Message ?? "Resend email send failed.",
            IsTransientFailure: response.StatusCode is HttpStatusCode.RequestTimeout
                or HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500 ||
                (response.StatusCode == HttpStatusCode.Conflict &&
                 error?.Name == "concurrent_idempotent_requests"),
            RetryAfter: GetRetryAfter(response));
    }

    private static TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is { } delta)
            return delta > TimeSpan.Zero ? delta : null;
        if (retryAfter?.Date is { } date)
        {
            TimeSpan delay = date - DateTimeOffset.UtcNow;
            return delay > TimeSpan.Zero ? delay : null;
        }
        return null;
    }

    private string ComposeFrom()
    {
        return string.IsNullOrWhiteSpace(_options.FromName)
            ? _options.FromEmail.Trim()
            : $"{_options.FromName.Trim()} <{_options.FromEmail.Trim()}>";
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
        [property: JsonPropertyName("reply_to")] string[]? ReplyTo);

    private sealed record ResendSendEmailResponse(string Id);

    private sealed record ResendErrorResponse(string Name, string Message);
}
