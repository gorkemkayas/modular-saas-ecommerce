using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Payments.Commands.ProcessPaymentWebhook;
using Payment.Domain.Enums;
using System.Text;

namespace ECommerce.API.Controllers.Payment;

[Route("api/payments/webhooks")]
[ApiController]
[AllowAnonymous]
public sealed class PaymentWebhooksController : ControllerBase
{
    private readonly ISender _sender;

    public PaymentWebhooksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{provider}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Receive(
        [FromRoute] string provider,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        if (!Enum.TryParse<PaymentProvider>(provider, ignoreCase: true, out var providerType))
            return BadRequest("Unsupported payment provider.");

        Request.Headers.TryGetValue("X-IYZ-SIGNATURE-V3", out var signature);
        if (string.IsNullOrWhiteSpace(signature.FirstOrDefault()))
            Request.Headers.TryGetValue("X-Signature", out signature);

        await _sender.Send(new ProcessPaymentWebhookCommand(
            providerType,
            payload,
            signature.FirstOrDefault()), cancellationToken);

        return Accepted();
    }
}
