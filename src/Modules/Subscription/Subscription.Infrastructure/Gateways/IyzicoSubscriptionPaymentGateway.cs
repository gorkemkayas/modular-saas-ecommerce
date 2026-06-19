using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Subscription.Application.Abstractions;
using Subscription.Infrastructure.Options;

namespace Subscription.Infrastructure.Gateways;

public sealed class IyzicoSubscriptionPaymentGateway : ISubscriptionPaymentGateway
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly SubscriptionIyzicoOptions _options;

    public IyzicoSubscriptionPaymentGateway(
        HttpClient httpClient,
        IOptions<SubscriptionIyzicoOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<SubscriptionCheckoutResult> InitializeCheckoutAsync(
        SubscriptionCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var conversationId = request.SubscriptionId.ToString("N");
        var formattedPrice = FormatPrice(request.Amount);

        var payload = new CheckoutFormInitializeRequest(
            _options.Locale,
            conversationId,
            formattedPrice,
            formattedPrice,
            request.Currency,
            $"SUB-{conversationId}",
            "SUBSCRIPTION",
            _options.CallbackUrl,
            new BuyerRequest(
                conversationId,
                GetFirstName(request.BuyerName),
                GetLastName(request.BuyerName),
                NormalizePhone(request.BuyerPhone),
                request.BuyerEmail,
                request.BuyerIdentityNumber,
                "Platform subscription",
                NormalizeIp(request.BuyerIpAddress),
                "Istanbul",
                "Turkey",
                "34000"),
            new AddressRequest("Platform", "Istanbul", "Turkey", "Platform subscription", "34000"),
            new AddressRequest("Platform", "Istanbul", "Turkey", "Platform subscription", "34000"),
            [
                new BasketItemRequest(
                    $"PLAN-{conversationId}",
                    request.PlanName,
                    "Subscription",
                    "VIRTUAL",
                    formattedPrice)
            ]);

        var response = await SendAsync<CheckoutFormInitializeRequest, CheckoutFormInitializeResponse>(
            _options.InitializeCheckoutFormPath,
            payload,
            cancellationToken);

        if (!IsSuccess(response.Status))
        {
            return new SubscriptionCheckoutResult(
                false, null, response.Token, response.ErrorCode, response.ErrorMessage);
        }

        return new SubscriptionCheckoutResult(
            true, response.PaymentPageUrl, response.Token, null, null);
    }

    public async Task<SubscriptionPaymentVerificationResult> VerifyPaymentAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<CheckoutFormRetrieveRequest, CheckoutFormRetrieveResponse>(
            _options.RetrieveCheckoutFormPath,
            new CheckoutFormRetrieveRequest(_options.Locale, token),
            cancellationToken);

        if (!IsSuccess(response.Status) ||
            !string.Equals(response.PaymentStatus, "SUCCESS", StringComparison.OrdinalIgnoreCase))
        {
            return new SubscriptionPaymentVerificationResult(
                false, response.PaymentId, response.ErrorCode, response.ErrorMessage);
        }

        return new SubscriptionPaymentVerificationResult(
            true, response.PaymentId, null, null);
    }

    private async Task<TResponse> SendAsync<TRequest, TResponse>(
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var requestBody = JsonSerializer.Serialize(request, SerializerOptions);
        var randomKey = Guid.NewGuid().ToString("N");
        var authorization = BuildAuthorizationHeader(path, requestBody, randomKey);

        using var message = new HttpRequestMessage(HttpMethod.Post, BuildRequestUri(path))
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        message.Headers.TryAddWithoutValidation("x-iyzi-rnd", randomKey);
        message.Headers.TryAddWithoutValidation("x-iyzi-client-version", "modular-saas-subscription");
        message.Headers.TryAddWithoutValidation("Authorization", authorization);

        var response = await _httpClient.SendAsync(message, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Iyzico subscription request failed ({(int)response.StatusCode}): {responseBody}");

        return JsonSerializer.Deserialize<TResponse>(responseBody, SerializerOptions)
            ?? throw new InvalidOperationException("Iyzico returned an empty response body.");
    }

    private string BuildAuthorizationHeader(string path, string requestBody, string randomKey)
    {
        var payload = $"{randomKey}{path}{requestBody}";
        var signatureBytes = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(_options.SecretKey),
            Encoding.UTF8.GetBytes(payload));
        var signature = Convert.ToHexString(signatureBytes).ToLowerInvariant();
        var authorizationBody = $"apiKey:{_options.ApiKey}&randomKey:{randomKey}&signature:{signature}";
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(authorizationBody));
        return $"IYZWSv2 {encoded}";
    }

    private Uri BuildRequestUri(string path)
    {
        var normalizedBaseUrl = _options.BaseUrl.TrimEnd('/') + "/";
        var normalizedPath = path.TrimStart('/');
        return new Uri(new Uri(normalizedBaseUrl, UriKind.Absolute), normalizedPath);
    }

    private static string FormatPrice(decimal amount)
    {
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero)
            .ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string NormalizePhone(string? phone) =>
        string.IsNullOrWhiteSpace(phone) ? "+905555555555" : phone.Trim();

    private static string NormalizeIp(string? ip) =>
        string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip.Trim();

    private static string GetFirstName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "Customer";
        return fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Customer";
    }

    private static string GetLastName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "Customer";
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? parts[^1] : "Customer";
    }

    private static bool IsSuccess(string? status) =>
        string.Equals(status, "success", StringComparison.OrdinalIgnoreCase);

    private sealed record CheckoutFormInitializeRequest(
        string Locale, string ConversationId, string Price, string PaidPrice,
        string Currency, string BasketId, string PaymentGroup, string CallbackUrl,
        BuyerRequest Buyer, AddressRequest ShippingAddress, AddressRequest BillingAddress,
        IReadOnlyCollection<BasketItemRequest> BasketItems);

    private sealed record BuyerRequest(
        string Id, string Name, string Surname, string GsmNumber, string Email,
        string IdentityNumber, string RegistrationAddress, string Ip,
        string City, string Country, string ZipCode);

    private sealed record AddressRequest(
        string ContactName, string City, string Country, string Address, string ZipCode);

    private sealed record BasketItemRequest(
        string Id, string Name, string Category1, string ItemType, string Price);

    private sealed record CheckoutFormInitializeResponse(
        string? Status, string? ConversationId, string? Token, string? PaymentPageUrl,
        string? ErrorCode, string? ErrorMessage);

    private sealed record CheckoutFormRetrieveRequest(string Locale, string Token);

    private sealed record CheckoutFormRetrieveResponse(
        string? Status, string? ConversationId, string? PaymentStatus, string? PaymentId,
        string? Token, string? ErrorCode, string? ErrorMessage,
        [property: JsonConverter(typeof(FlexibleStringJsonConverter))] string? Price,
        [property: JsonConverter(typeof(FlexibleStringJsonConverter))] string? PaidPrice);

    private sealed class FlexibleStringJsonConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Null => null,
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.GetDecimal().ToString(CultureInfo.InvariantCulture),
                _ => JsonDocument.ParseValue(ref reader).RootElement.ToString()
            };
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (value is null) writer.WriteNullValue();
            else writer.WriteStringValue(value);
        }
    }
}
