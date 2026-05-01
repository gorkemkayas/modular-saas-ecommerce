using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.Notifications.Services;
using Notification.Infrastructure.Options;

namespace Notification.Infrastructure.Services;

public sealed class ResendWebhookVerifier : IResendWebhookVerifier
{
    private const string SecretPrefix = "whsec_";
    private static readonly TimeSpan TimestampTolerance = TimeSpan.FromMinutes(5);

    private readonly byte[]? _secretKey;
    private readonly ILogger<ResendWebhookVerifier> _logger;

    public ResendWebhookVerifier(
        IOptions<NotificationEmailOptions> options,
        ILogger<ResendWebhookVerifier> logger)
    {
        _logger = logger;

        var configuredSecret = options.Value.WebhookSecret?.Trim();
        if (string.IsNullOrWhiteSpace(configuredSecret))
        {
            IsVerificationEnabled = false;
            return;
        }

        try
        {
            var normalizedSecret = configuredSecret.StartsWith(SecretPrefix, StringComparison.OrdinalIgnoreCase)
                ? configuredSecret[SecretPrefix.Length..]
                : configuredSecret;

            _secretKey = Convert.FromBase64String(normalizedSecret);
            IsVerificationEnabled = true;
        }
        catch (FormatException exception)
        {
            _logger.LogWarning(
                exception,
                "Notification webhook verification is disabled because the Resend webhook secret is not valid base64.");
            IsVerificationEnabled = false;
        }
    }

    public bool IsVerificationEnabled { get; }

    public bool Verify(string payload, string? webhookId, string? webhookTimestamp, string? webhookSignature)
    {
        if (!IsVerificationEnabled || _secretKey is null)
            return true;

        if (string.IsNullOrWhiteSpace(payload)
            || string.IsNullOrWhiteSpace(webhookId)
            || string.IsNullOrWhiteSpace(webhookTimestamp)
            || string.IsNullOrWhiteSpace(webhookSignature))
        {
            return false;
        }

        if (!long.TryParse(webhookTimestamp, out var unixTimestamp))
            return false;

        var eventTimestamp = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
        var now = DateTimeOffset.UtcNow;

        if (eventTimestamp < now - TimestampTolerance || eventTimestamp > now + TimestampTolerance)
            return false;

        var signedPayload = $"{webhookId}.{unixTimestamp}.{payload}";
        var signatureBytes = Encoding.UTF8.GetBytes(signedPayload);

        using var hmac = new HMACSHA256(_secretKey);
        var computedSignature = Convert.ToBase64String(hmac.ComputeHash(signatureBytes));

        foreach (var candidate in webhookSignature.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = candidate.Split(',', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !parts[0].Equals("v1", StringComparison.OrdinalIgnoreCase))
                continue;

            if (TimingSafeEquals(computedSignature, parts[1]))
                return true;
        }

        return false;
    }

    private static bool TimingSafeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        return leftBytes.Length == rightBytes.Length
            && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
