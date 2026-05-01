using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using ECommerce.API.Contracts.Payment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Common.Models;
using Payment.Application.Payments.Commands.CancelPayment;
using Payment.Application.Payments.Commands.CapturePayment;
using Payment.Application.Payments.Commands.RefundPayment;
using Payment.Application.Payments.DTOs;
using Payment.Application.Payments.Queries.GetStorePaymentById;
using Payment.Application.Payments.Queries.SearchStorePayments;

namespace ECommerce.API.Controllers.Payment;

[Route("api/stores/me/payments")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
public sealed class StorePaymentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public StorePaymentsController(ISender sender, ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PaymentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] SearchPaymentsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SearchStorePaymentsQuery(
            GetStoreId(),
            request.Status,
            request.PageNumber,
            request.PageSize), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{paymentId:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid paymentId,
        CancellationToken cancellationToken)
    {
        var payment = await _sender.Send(new GetStorePaymentByIdQuery(GetStoreId(), paymentId), cancellationToken);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPost("{paymentId:guid}/capture")]
    [ProducesResponseType(typeof(PaymentActionResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Capture(
        [FromRoute] Guid paymentId,
        [FromBody] CapturePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CapturePaymentCommand(GetStoreId(), paymentId, request.IdempotencyKey), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{paymentId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid paymentId,
        [FromBody] CancelPaymentRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new CancelPaymentCommand(GetStoreId(), paymentId, request.IdempotencyKey), cancellationToken);
        return NoContent();
    }

    [HttpPost("{paymentId:guid}/refund")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Refund(
        [FromRoute] Guid paymentId,
        [FromBody] RefundPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RefundPaymentCommand(
            GetStoreId(),
            paymentId,
            request.Amount,
            request.Reason,
            request.IdempotencyKey), cancellationToken);

        return Ok(result);
    }

    private Guid GetStoreId() => _tenantContext.TenantIdAsGuid ?? Guid.Empty;
}
