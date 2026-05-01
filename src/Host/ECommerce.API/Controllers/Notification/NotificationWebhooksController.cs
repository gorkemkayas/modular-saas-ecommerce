using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Notifications.Commands.ProcessResendWebhook;

namespace ECommerce.API.Controllers.Notification;

[Route("api/notifications/webhooks")]
[ApiController]
[AllowAnonymous]
public sealed class NotificationWebhooksController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationWebhooksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("resend")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ReceiveResendWebhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        Request.Headers.TryGetValue("svix-id", out var webhookId);
        Request.Headers.TryGetValue("svix-timestamp", out var webhookTimestamp);
        Request.Headers.TryGetValue("svix-signature", out var webhookSignature);

        await _sender.Send(
            new ProcessResendWebhookCommand(
                payload,
                webhookId.FirstOrDefault(),
                webhookTimestamp.FirstOrDefault(),
                webhookSignature.FirstOrDefault()),
            cancellationToken);

        return Accepted();
    }
}
