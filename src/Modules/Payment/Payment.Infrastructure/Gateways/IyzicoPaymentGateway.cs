using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Payment.Application.Integrations;
using Payment.Domain.Enums;
using Payment.Infrastructure.Options;

namespace Payment.Infrastructure.Gateways;

public sealed class IyzicoPaymentGateway : IPaymentGateway
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly IyzicoOptions _options;

    public IyzicoPaymentGateway(
        HttpClient httpClient,
        IOptions<IyzicoOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public PaymentProvider Provider => PaymentProvider.Iyzico;

    public async Task<PaymentGatewayOperationResult> AuthorizeAsync(
        PaymentGatewayAuthorizeRequest request,
        CancellationToken cancellationToken = default)
    {
        var conversationId = request.PaymentId.ToString("N");
        var buyerIdentityNumber = ResolveBuyerIdentityNumber();

        var payload = new CheckoutFormInitializeRequest(
            _options.Locale,
            conversationId,
            FormatPrice(request.Amount),
            FormatPrice(request.Amount),
            request.CurrencyCode,
            request.OrderId.ToString("N"),
            "PRODUCT",
            _options.CallbackUrl,
            new CheckoutFormBuyerRequest(
                request.CustomerId.ToString("N"),
                GetFirstName(request.Customer.FullName),
                GetLastName(request.Customer.FullName),
                NormalizePhoneNumber(request.Customer.PhoneNumber ?? request.BillingAddress.PhoneNumber),
                request.Customer.Email,
                buyerIdentityNumber,
                ComposeAddress(request.BillingAddress),
                NormalizeIpAddress(request.ClientIpAddress),
                request.BillingAddress.City,
                request.BillingAddress.Country,
                request.BillingAddress.PostalCode ?? "34000"),
            new CheckoutFormAddressRequest(
                request.ShippingAddress.ContactName,
                request.ShippingAddress.City,
                request.ShippingAddress.Country,
                ComposeAddress(request.ShippingAddress),
                request.ShippingAddress.PostalCode ?? "34000"),
            new CheckoutFormAddressRequest(
                request.BillingAddress.ContactName,
                request.BillingAddress.City,
                request.BillingAddress.Country,
                ComposeAddress(request.BillingAddress),
                request.BillingAddress.PostalCode ?? "34000"),
            request.Items.Select(MapBasketItem).ToArray());

        var response = await SendAsync<CheckoutFormInitializeRequest, CheckoutFormInitializeResponse>(
            HttpMethod.Post,
            _options.InitializeCheckoutFormPath,
            payload,
            cancellationToken);

        ValidateCheckoutFormInitializeSignature(response);

        if (!IsSuccess(response.Status))
        {
            return new PaymentGatewayOperationResult(
                PaymentGatewayOutcome.Failed,
                null,
                response.ConversationId,
                null,
                response.ErrorCode,
                response.ErrorMessage,
                response.Token);
        }

        return new PaymentGatewayOperationResult(
            PaymentGatewayOutcome.RequiresAction,
            null,
            response.ConversationId,
            response.PaymentPageUrl,
            null,
            null,
            response.Token);
    }

    public async Task<PaymentGatewayOperationResult> CompleteAsync(
        PaymentGatewayCompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<CheckoutFormRetrieveRequest, CheckoutFormRetrieveResponse>(
            HttpMethod.Post,
            _options.RetrieveCheckoutFormPath,
            new CheckoutFormRetrieveRequest(_options.Locale, request.Token),
            cancellationToken);

        ValidateCheckoutFormRetrieveSignature(response);

        if (!IsSuccess(response.Status))
        {
            return new PaymentGatewayOperationResult(
                PaymentGatewayOutcome.Failed,
                response.PaymentId,
                response.ConversationId,
                null,
                response.ErrorCode,
                response.ErrorMessage,
                request.Token);
        }

        return new PaymentGatewayOperationResult(
            MapCheckoutOutcome(response.PaymentStatus, response.FraudStatus),
            response.PaymentId,
            response.ConversationId,
            null,
            response.ErrorCode,
            response.ErrorMessage,
            request.Token);
    }

    public Task<PaymentGatewayOperationResult> CaptureAsync(
        PaymentGatewayCaptureRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Iyzico Checkout Form flow completes payment through the hosted checkout callback. Separate capture is not supported in this integration.");
    }

    public async Task<PaymentGatewayOperationResult> CancelAsync(
        PaymentGatewayCancelRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<CancelPaymentRequest, BasicGatewayResponse>(
            HttpMethod.Post,
            _options.CancelPath,
            new CancelPaymentRequest(
                _options.Locale,
                request.ExternalPaymentReference ?? throw new InvalidOperationException("External payment reference is required for cancel."),
                request.IdempotencyKey,
                request.ExternalConversationId ?? request.PaymentId.ToString("N")),
            cancellationToken);

        return new PaymentGatewayOperationResult(
            IsSuccess(response.Status) ? PaymentGatewayOutcome.Cancelled : PaymentGatewayOutcome.Failed,
            request.ExternalPaymentReference,
            request.ExternalConversationId,
            null,
            response.ErrorCode,
            response.ErrorMessage,
            request.IdempotencyKey);
    }

    public async Task<PaymentGatewayOperationResult> RefundAsync(
        PaymentGatewayRefundRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<RefundPaymentRequest, RefundPaymentResponse>(
            HttpMethod.Post,
            _options.RefundPath,
            new RefundPaymentRequest(
                _options.Locale,
                request.ExternalPaymentReference ?? throw new InvalidOperationException("External payment reference is required for refund."),
                FormatPrice(request.RefundAmount),
                request.IdempotencyKey,
                request.ExternalConversationId ?? request.PaymentId.ToString("N")),
            cancellationToken);

        return new PaymentGatewayOperationResult(
            IsSuccess(response.Status) ? PaymentGatewayOutcome.Refunded : PaymentGatewayOutcome.Failed,
            request.ExternalPaymentReference,
            request.ExternalConversationId,
            null,
            response.ErrorCode,
            response.ErrorMessage,
            request.IdempotencyKey,
            IsSuccess(response.Status) ? request.RefundAmount : null);
    }

    private async Task<TResponse> SendAsync<TRequest, TResponse>(
        HttpMethod method,
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var requestBody = JsonSerializer.Serialize(request, SerializerOptions);
        var randomKey = Guid.NewGuid().ToString("N");
        var authorization = BuildAuthorizationHeader(path, requestBody, randomKey);

        using var message = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        message.Headers.TryAddWithoutValidation("x-iyzi-rnd", randomKey);
        message.Headers.TryAddWithoutValidation("x-iyzi-client-version", "modular-saas-ecommerce-payment");
        message.Headers.TryAddWithoutValidation("Authorization", authorization);

        var response = await _httpClient.SendAsync(message, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<TResponse>(responseBody, SerializerOptions)
            ?? throw new InvalidOperationException("Iyzico returned an empty response body.");
    }

    private string BuildAuthorizationHeader(string path, string requestBody, string randomKey)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new InvalidOperationException("Iyzico ApiKey and SecretKey must be configured.");

        var payload = $"{randomKey}{path}{requestBody}";
        var signatureBytes = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(_options.SecretKey),
            Encoding.UTF8.GetBytes(payload));
        var signature = Convert.ToHexString(signatureBytes).ToLowerInvariant();
        var authorizationBody = $"apiKey:{_options.ApiKey}&randomKey:{randomKey}&signature:{signature}";
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(authorizationBody));
        return $"IYZWSv2 {encoded}";
    }

    private void ValidateCheckoutFormInitializeSignature(CheckoutFormInitializeResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Signature) || string.IsNullOrWhiteSpace(_options.SecretKey))
            return;

        var expected = ComputeResponseSignature(
            _options.SecretKey,
            response.ConversationId,
            response.Token);

        if (!FixedTimeEquals(expected, response.Signature))
            throw new InvalidOperationException("Iyzico checkout form initialize signature validation failed.");
    }

    private void ValidateCheckoutFormRetrieveSignature(CheckoutFormRetrieveResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Signature) || string.IsNullOrWhiteSpace(_options.SecretKey))
            return;

        var expected = ComputeResponseSignature(
            _options.SecretKey,
            response.PaymentStatus,
            response.PaymentId,
            response.Currency,
            response.BasketId,
            response.ConversationId,
            NormalizeSignedPrice(response.PaidPrice),
            NormalizeSignedPrice(response.Price),
            response.Token);

        if (!FixedTimeEquals(expected, response.Signature))
            throw new InvalidOperationException("Iyzico checkout form retrieve signature validation failed.");
    }

    private static string ComputeResponseSignature(string secretKey, params string?[] values)
    {
        var serialized = string.Join(
            ":",
            values.Select(value => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim()));

        var signatureBytes = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secretKey),
            Encoding.UTF8.GetBytes(serialized));

        return Convert.ToHexString(signatureBytes).ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string expected, string actual)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(actual.Trim().ToLowerInvariant()));
    }

    private static CheckoutFormBasketItemRequest MapBasketItem(PaymentGatewayBasketItem item)
    {
        var displayName = string.IsNullOrWhiteSpace(item.VariantName)
            ? item.ProductName
            : $"{item.ProductName} - {item.VariantName}";

        return new CheckoutFormBasketItemRequest(
            item.ProductId.ToString("N"),
            displayName,
            "General",
            "PHYSICAL",
            FormatPrice(item.LineTotalAmount));
    }

    private string ResolveBuyerIdentityNumber()
    {
        if (string.IsNullOrWhiteSpace(_options.DefaultBuyerIdentityNumber))
            throw new InvalidOperationException("Iyzico DefaultBuyerIdentityNumber must be configured for Checkout Form flow.");

        return _options.DefaultBuyerIdentityNumber;
    }

    private static PaymentGatewayOutcome MapCheckoutOutcome(string? paymentStatus, int? fraudStatus)
    {
        if (!string.Equals(paymentStatus, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            return PaymentGatewayOutcome.Failed;

        return fraudStatus switch
        {
            1 => PaymentGatewayOutcome.Captured,
            0 => PaymentGatewayOutcome.Authorized,
            _ => PaymentGatewayOutcome.Failed
        };
    }

    private static string ComposeAddress(PaymentGatewayAddress address)
    {
        return string.Join(", ", new[]
        {
            address.Line1,
            address.Line2,
            address.District,
            address.City,
            address.Country
        }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string NormalizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return "+905555555555";

        return phoneNumber.Trim();
    }

    private static string NormalizeIpAddress(string? ipAddress)
    {
        return string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress.Trim();
    }

    private static string GetFirstName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "Customer";

        return fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Customer";
    }

    private static string GetLastName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "Customer";

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? parts[^1] : "Customer";
    }

    private static string FormatPrice(decimal amount)
    {
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero)
            .ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string NormalizeSignedPrice(string? price)
    {
        if (string.IsNullOrWhiteSpace(price))
            return string.Empty;

        if (!decimal.TryParse(price, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            return price.Trim();

        return decimal.Round(parsed, 2, MidpointRounding.AwayFromZero)
            .ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static bool IsSuccess(string? status)
    {
        return string.Equals(status, "success", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record CheckoutFormInitializeRequest(
        string Locale,
        string ConversationId,
        string Price,
        string PaidPrice,
        string Currency,
        string BasketId,
        string PaymentGroup,
        string CallbackUrl,
        CheckoutFormBuyerRequest Buyer,
        CheckoutFormAddressRequest ShippingAddress,
        CheckoutFormAddressRequest BillingAddress,
        IReadOnlyCollection<CheckoutFormBasketItemRequest> BasketItems);

    private sealed record CheckoutFormBuyerRequest(
        string Id,
        string Name,
        string Surname,
        string GsmNumber,
        string Email,
        string IdentityNumber,
        string RegistrationAddress,
        string Ip,
        string City,
        string Country,
        string ZipCode);

    private sealed record CheckoutFormAddressRequest(
        string ContactName,
        string City,
        string Country,
        string Address,
        string ZipCode);

    private sealed record CheckoutFormBasketItemRequest(
        string Id,
        string Name,
        string Category1,
        string ItemType,
        string Price);

    private sealed record CheckoutFormInitializeResponse(
        string? Status,
        string? ConversationId,
        string? Token,
        string? PaymentPageUrl,
        string? ErrorCode,
        string? ErrorMessage,
        string? Signature);

    private sealed record CheckoutFormRetrieveRequest(
        string Locale,
        string Token);

    private sealed record CheckoutFormRetrieveResponse(
        string? Status,
        string? ConversationId,
        string? PaymentStatus,
        string? PaymentId,
        string? Token,
        string? Currency,
        string? BasketId,
        string? Price,
        string? PaidPrice,
        int? FraudStatus,
        string? ErrorCode,
        string? ErrorMessage,
        string? Signature);

    private sealed record CancelPaymentRequest(
        string Locale,
        string PaymentId,
        string ConversationId,
        string PaymentConversationId);

    private sealed record RefundPaymentRequest(
        string Locale,
        string PaymentTransactionId,
        string Price,
        string ConversationId,
        string PaymentConversationId);

    private sealed record BasicGatewayResponse(
        string? Status,
        string? ErrorCode,
        string? ErrorMessage);

    private sealed record RefundPaymentResponse(
        string? Status,
        string? ErrorCode,
        string? ErrorMessage);
}
