using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ECommerce.API.Options;
using Payment.Application.Payments.Commands.CompletePaymentCheckout;
using Payment.Application.Payments.DTOs;
using Payment.Domain.Enums;
using Store.Application.DTOs;
using Store.Application.Stores.Queries.GetStoreById;
using Store.Application.Stores.Queries.GetStoreByTenantId;
using System.Text.Encodings.Web;

namespace ECommerce.API.Controllers.Payment;

[Route("api/payments/callbacks")]
[ApiController]
[AllowAnonymous]
public sealed class PaymentCallbacksController : ControllerBase
{
    private readonly ISender _sender;
    private readonly FrontendOptions _frontendOptions;

    public PaymentCallbacksController(ISender sender, IOptions<FrontendOptions> frontendOptions)
    {
        _sender = sender;
        _frontendOptions = frontendOptions.Value;
    }

    [HttpPost("iyzico/checkout-form")]
    [HttpGet("iyzico/checkout-form")]
    [ProducesResponseType(typeof(PaymentActionResultDto), StatusCodes.Status200OK)]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
    public async Task<IActionResult> CompleteIyzicoCheckoutForm(CancellationToken cancellationToken)
    {
        var token = await ResolveTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return Content(BuildMissingTokenHtml(), "text/html; charset=utf-8");

        var result = await _sender.Send(
            new CompletePaymentCheckoutCommand(PaymentProvider.Iyzico, token),
            cancellationToken);

        var store = await ResolveStoreAsync(result.StoreId, cancellationToken);
        if (store is null)
            return NotFound("Store could not be resolved for the completed payment.");

        var redirectUrl = BuildPaymentResultUrl(store.Slug, result);

        Response.StatusCode = StatusCodes.Status303SeeOther;
        Response.Headers.Location = redirectUrl;
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";

        return Content(
            BuildAutoRedirectHtml(redirectUrl),
            "text/html; charset=utf-8");
    }

    private async Task<string?> ResolveTokenAsync(CancellationToken cancellationToken)
    {
        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync(cancellationToken);
            if (form.TryGetValue("token", out var formToken))
                return formToken.FirstOrDefault();
        }

        if (Request.Query.TryGetValue("token", out var queryToken))
            return queryToken.FirstOrDefault();

        return null;
    }

    private string BuildPaymentResultUrl(string storeSlug, PaymentActionResultDto result)
    {
        var status = MapPaymentStatus(result.Status);
        var query = new QueryString()
            .Add("status", status)
            .Add("orderId", result.OrderId.ToString());

        var relativePath = $"/{Uri.EscapeDataString(storeSlug)}/payment-result{query}";
        var baseUrl = _frontendOptions.BaseUrl?.Trim().TrimEnd('/');

        return string.IsNullOrWhiteSpace(baseUrl)
            ? relativePath
            : $"{baseUrl}{relativePath}";
    }

    private static string MapPaymentStatus(PaymentStatus status) =>
        status switch
        {
            PaymentStatus.Authorized => "success",
            PaymentStatus.Captured => "success",
            PaymentStatus.Refunded => "success",
            PaymentStatus.PartiallyRefunded => "success",
            PaymentStatus.Failed => "failed",
            PaymentStatus.Cancelled => "failed",
            PaymentStatus.Pending => "pending",
            PaymentStatus.RequiresAction => "processing",
            _ => "processing"
        };

    private static string BuildAutoRedirectHtml(string targetUrl)
    {
        var encodedUrl = HtmlEncoder.Default.Encode(targetUrl);
        var javascriptUrl = JavaScriptEncoder.Default.Encode(targetUrl);

        return $$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta http-equiv="refresh" content="0;url={{encodedUrl}}" />
    <title>Redirecting…</title>
</head>
<body>
    <script>
        (function () {
            var targetUrl = "{{javascriptUrl}}";
            if (window.top && window.top !== window) {
                window.top.location.replace(targetUrl);
                return;
            }

            window.location.replace(targetUrl);
        })();
    </script>
    <p>Redirecting to the payment result page…</p>
    <p><a href="{{encodedUrl}}">Continue</a></p>
</body>
</html>
""";
    }

    private static string BuildMissingTokenHtml()
    {
        return """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <title>Payment callback could not be completed</title>
</head>
<body>
    <h1>Payment callback could not be completed</h1>
    <p>The payment provider returned to the callback URL without a checkout token.</p>
    <p>This usually means the browser or tunnel blocked the cross-origin POST that should have delivered the token.</p>
    <p>Please retry with a stable public HTTPS tunnel for the API callback endpoint.</p>
</body>
</html>
""";
    }

    private async Task<StoreDto?> ResolveStoreAsync(Guid storeOrTenantId, CancellationToken cancellationToken)
    {
        var store = await _sender.Send(new GetStoreByIdQuery(storeOrTenantId), cancellationToken);
        if (store is not null)
            return store;

        return await _sender.Send(new GetStoreByTenantIdQuery(storeOrTenantId), cancellationToken);
    }
}
