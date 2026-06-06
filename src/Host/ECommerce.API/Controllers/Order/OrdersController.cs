using BuildingBlocks.Application.Abstractions.Authentication;
using BuildingBlocks.Application.Abstractions.Tenancy;
using ECommerce.API.Contracts.Order;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Common.Models;
using Order.Application.Orders.Commands.CancelOrder;
using Order.Application.Orders.Commands.PlaceOrder;
using Order.Application.Orders.DTOs;
using Order.Application.Orders.Queries.GetMyOrders;
using Order.Application.Orders.Queries.GetOrderById;

namespace ECommerce.API.Controllers.Order;

[Route("api/orders")]
[ApiController]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public OrdersController(
        ISender sender,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _sender = sender;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var storeId, out var externalUserId))
            return Unauthorized();

        var orderId = await _sender.Send(new PlaceOrderCommand(
            storeId,
            externalUserId,
            request.ShippingAddressId,
            request.BillingAddressId,
            request.ShippingCarrierId,
            request.CurrencyCode,
            request.Items
                .Select(x => new PlaceOrderItemInput(x.ProductId, x.ProductVariantId, x.Quantity))
                .ToArray()), cancellationToken);

        return CreatedAtAction(nameof(GetMyOrderById), new { orderId }, new { OrderId = orderId });
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(PagedResult<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] GetMyOrdersRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var storeId, out var externalUserId))
            return Unauthorized();

        var result = await _sender.Send(new GetMyOrdersQuery(
            storeId,
            externalUserId,
            request.PageNumber,
            request.PageSize), cancellationToken);

        return Ok(result);
    }

    [HttpGet("me/{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyOrderById(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var storeId, out var externalUserId))
            return Unauthorized();

        var order = await _sender.Send(new GetOrderByIdQuery(storeId, externalUserId, orderId), cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{orderId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelOrder(
        [FromRoute] Guid orderId,
        [FromBody] CancelOrderRequest? request,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var storeId, out var externalUserId))
            return Unauthorized();

        await _sender.Send(new CancelOrderCommand(
            storeId,
            externalUserId,
            orderId,
            request?.Reason), cancellationToken);

        return NoContent();
    }

    private bool TryGetContext(out Guid storeId, out Guid externalUserId)
    {
        storeId = _tenantContext.TenantIdAsGuid ?? Guid.Empty;
        externalUserId = _currentUser.UserId ?? Guid.Empty;

        return storeId != Guid.Empty && externalUserId != Guid.Empty && _currentUser.IsAuthenticated;
    }
}
