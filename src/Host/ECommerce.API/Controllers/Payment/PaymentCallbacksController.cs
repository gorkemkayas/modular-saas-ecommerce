using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Payments.Commands.CompletePaymentCheckout;
using Payment.Application.Payments.DTOs;
using Payment.Domain.Enums;

namespace ECommerce.API.Controllers.Payment;

[Route("api/payments/callbacks")]
[ApiController]
[AllowAnonymous]
public sealed class PaymentCallbacksController : ControllerBase
{
    private readonly ISender _sender;

    public PaymentCallbacksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("iyzico/checkout-form")]
    [ProducesResponseType(typeof(PaymentActionResultDto), StatusCodes.Status200OK)]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
    public async Task<IActionResult> CompleteIyzicoCheckoutForm(CancellationToken cancellationToken)
    {
        var token = await ResolveTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Checkout token is required.");

        var result = await _sender.Send(
            new CompletePaymentCheckoutCommand(PaymentProvider.Iyzico, token),
            cancellationToken);

        return Ok(result);
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
}
