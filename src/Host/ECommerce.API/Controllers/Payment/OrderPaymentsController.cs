using BuildingBlocks.Application.Abstractions.Authentication;
using BuildingBlocks.Application.Abstractions.Tenancy;
using ECommerce.API.Contracts.Payment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Payments.Commands.AuthorizePayment;
using Payment.Application.Payments.Commands.CreatePaymentForOrder;
using Payment.Application.Payments.DTOs;
using Payment.Application.Payments.Queries.GetMyPaymentByOrderId;

namespace ECommerce.API.Controllers.Payment;

[Route("api/orders/{orderId:guid}/payment")]
[ApiController]
[Authorize]
public sealed class OrderPaymentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public OrderPaymentsController(
        ISender sender,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _sender = sender;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid orderId,
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var storeId, out var externalUserId))
            return Unauthorized();

        var paymentId = await _sender.Send(new CreatePaymentForOrderCommand(
            storeId,
            externalUserId,
            orderId,
            request.MethodType), cancellationToken);

        return CreatedAtAction(nameof(GetByOrderId), new { orderId }, new { PaymentId = paymentId });
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrderId(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var storeId, out var externalUserId))
            return Unauthorized();

        var payment = await _sender.Send(new GetMyPaymentByOrderIdQuery(
            storeId,
            externalUserId,
            orderId), cancellationToken);

        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPost("authorize")]
    [ProducesResponseType(typeof(PaymentActionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Authorize(
        [FromRoute] Guid orderId,
        [FromBody] AuthorizePaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var storeId, out var externalUserId))
            return Unauthorized();

        var result = await _sender.Send(new AuthorizePaymentCommand(
            storeId,
            externalUserId,
            orderId,
            request.IdempotencyKey,
            ResolveClientIpAddress()), cancellationToken);

        return Ok(result);
    }

    private string? ResolveClientIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var raw = forwardedFor.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(raw))
                return raw.Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private bool TryGetContext(out Guid storeId, out Guid externalUserId)
    {
        storeId = _tenantContext.TenantIdAsGuid ?? Guid.Empty;
        externalUserId = _currentUser.UserId ?? Guid.Empty;

        return storeId != Guid.Empty && externalUserId != Guid.Empty && _currentUser.IsAuthenticated;
    }
}
