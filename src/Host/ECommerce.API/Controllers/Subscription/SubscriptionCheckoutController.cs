using BuildingBlocks.Application.Extensions;
using ECommerce.API.Contracts.Subscription;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Stores.Commands.ActivateStore;
using Store.Application.Stores.Commands.PublishStore;
using Microsoft.Extensions.Configuration;
using Store.Application.Stores.Commands.DeletePendingStore;
using Subscription.Contracts;

namespace ECommerce.API.Controllers.Subscription;

[Route("api/subscription/checkout")]
[ApiController]
[AllowAnonymous]
public sealed class SubscriptionCheckoutController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ISubscriptionModuleApi _subscriptionModuleApi;
    private readonly IConfiguration _configuration;

    public SubscriptionCheckoutController(
        ISender sender,
        ISubscriptionModuleApi subscriptionModuleApi,
        IConfiguration configuration)
    {
        _sender = sender;
        _subscriptionModuleApi = subscriptionModuleApi;
        _configuration = configuration;
    }

    [HttpPost("initiate")]
    [ProducesResponseType(typeof(SubscriptionCheckoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiateCheckout(
        [FromBody] SubscriptionCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var tenantGuid = TenantIdConverter.ToGuid(request.TenantId);

        var subscription = await _subscriptionModuleApi.GetTenantSubscriptionAsync(
            new GetTenantSubscriptionRequest(tenantGuid), cancellationToken);

        if (subscription is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Subscription not found.",
                Detail = "No subscription found for this tenant."
            });
        }

        if (subscription.Status != "PendingPayment")
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid subscription status.",
                Detail = "Subscription is already active or not in a payable state."
            });
        }

        var checkoutResult = await _subscriptionModuleApi.InitiateCheckoutAsync(
            new InitiateSubscriptionCheckoutRequest(
                tenantGuid,
                request.PlanCode,
                "Store",
                "store",
                request.BuyerEmail,
                request.BuyerName,
                request.BuyerPhone,
                request.BuyerIdentityNumber,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1"),
            cancellationToken);

        return Ok(new SubscriptionCheckoutResponse(
            checkoutResult.SubscriptionId,
            checkoutResult.PaymentPageUrl,
            checkoutResult.Token));
    }

    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompletePayment(
        [FromForm] string token,
        CancellationToken cancellationToken)
    {
        var result = await _subscriptionModuleApi.CompletePaymentAsync(
            new CompleteSubscriptionPaymentRequest(token),
            cancellationToken);

        var frontendBaseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:3000";

        if (!result.IsSuccess)
        {
            await _sender.Send(new DeletePendingStoreCommand(result.TenantId), cancellationToken);

            var failUrl = $"{frontendBaseUrl}/store-register/payment-result?status=failed&error={Uri.EscapeDataString(result.ErrorMessage ?? "Payment failed")}";
            return Redirect(failUrl);
        }

        await _sender.Send(new ActivateStoreCommand(result.TenantId), cancellationToken);
        await _sender.Send(new PublishStoreCommand(result.TenantId), cancellationToken);

        var successUrl = $"{frontendBaseUrl}/store-register/payment-result?status=success&planCode={Uri.EscapeDataString(result.PlanCode ?? "")}";
        return Redirect(successUrl);
    }
}
