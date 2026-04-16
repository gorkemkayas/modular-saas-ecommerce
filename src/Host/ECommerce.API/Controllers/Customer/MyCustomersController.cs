using BuildingBlocks.Application.Abstractions.Authentication;
using BuildingBlocks.Application.Abstractions.Tenancy;
using Customer.Application.Customers.Commands.AddAddress;
using Customer.Application.Customers.Commands.RemoveAddress;
using Customer.Application.Customers.Commands.SetDefaultBillingAddress;
using Customer.Application.Customers.Commands.SetDefaultShippingAddress;
using Customer.Application.Customers.Commands.UpdateAddress;
using Customer.Application.Customers.Commands.UpdateMyProfile;
using Customer.Application.Customers.Commands.UpdatePreferences;
using Customer.Application.Customers.Commands.UpsertConsent;
using Customer.Application.Customers.DTOs;
using Customer.Application.Customers.Queries.GetMyProfile;
using Customer.Domain.Enums;
using ECommerce.API.Contracts.Customer.Addresses;
using ECommerce.API.Contracts.Customer.Consents;
using ECommerce.API.Contracts.Customer.Preferences;
using ECommerce.API.Contracts.Customer.Profile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Customer;

[Route("api/customers/me")]
[ApiController]
[Authorize]
public sealed class MyCustomersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public MyCustomersController(
        ISender sender,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _sender = sender;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var tenantId, out var externalUserId))
            return Unauthorized();

        var customer = await _sender.Send(new GetMyProfileQuery(tenantId, externalUserId), cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateMyCustomerProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var tenantId, out var externalUserId))
            return Unauthorized();

        await _sender.Send(new UpdateMyProfileCommand(
            tenantId,
            externalUserId,
            request.FirstName,
            request.LastName,
            request.PhoneNumber), cancellationToken);

        return NoContent();
    }

    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateCustomerPreferencesRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var tenantId, out var externalUserId))
            return Unauthorized();

        await _sender.Send(new UpdatePreferencesCommand(
            tenantId,
            externalUserId,
            request.PreferredLanguage,
            request.PreferredCurrency), cancellationToken);

        return NoContent();
    }

    [HttpPost("addresses")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddAddress(
        [FromBody] AddCustomerAddressRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var tenantId, out var externalUserId))
            return Unauthorized();

        var addressId = await _sender.Send(new AddAddressCommand(
            tenantId,
            externalUserId,
            request.AddressType,
            request.Title,
            request.ContactName,
            request.PhoneNumber,
            request.Country,
            request.City,
            request.District,
            request.Line1,
            request.Line2,
            request.PostalCode,
            request.IsDefaultShipping,
            request.IsDefaultBilling), cancellationToken);

        return CreatedAtAction(nameof(GetMyProfile), new { }, new { AddressId = addressId });
    }

    [HttpPut("addresses/{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateAddress(
        [FromRoute] Guid addressId,
        [FromBody] UpdateCustomerAddressRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var tenantId, out var externalUserId))
            return Unauthorized();

        await _sender.Send(new UpdateAddressCommand(
            tenantId,
            externalUserId,
            addressId,
            request.AddressType,
            request.Title,
            request.ContactName,
            request.PhoneNumber,
            request.Country,
            request.City,
            request.District,
            request.Line1,
            request.Line2,
            request.PostalCode), cancellationToken);

        return NoContent();
    }

    [HttpDelete("addresses/{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveAddress([FromRoute] Guid addressId, CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var tenantId, out var externalUserId))
            return Unauthorized();

        await _sender.Send(new RemoveAddressCommand(tenantId, externalUserId, addressId), cancellationToken);
        return NoContent();
    }

    [HttpPost("addresses/{addressId:guid}/default-shipping")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetDefaultShippingAddress([FromRoute] Guid addressId, CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var tenantId, out var externalUserId))
            return Unauthorized();

        await _sender.Send(new SetDefaultShippingAddressCommand(tenantId, externalUserId, addressId), cancellationToken);
        return NoContent();
    }

    [HttpPost("addresses/{addressId:guid}/default-billing")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetDefaultBillingAddress([FromRoute] Guid addressId, CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var tenantId, out var externalUserId))
            return Unauthorized();

        await _sender.Send(new SetDefaultBillingAddressCommand(tenantId, externalUserId, addressId), cancellationToken);
        return NoContent();
    }

    [HttpPut("consents/{consentType}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpsertConsent(
        [FromRoute] ConsentType consentType,
        [FromBody] UpdateCustomerConsentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var tenantId, out var externalUserId))
            return Unauthorized();

        await _sender.Send(new UpsertConsentCommand(
            tenantId,
            externalUserId,
            consentType,
            request.IsGranted,
            request.Source), cancellationToken);

        return NoContent();
    }

    private bool TryGetContext(out Guid tenantId, out Guid externalUserId)
    {
        tenantId = _tenantContext.TenantIdAsGuid ?? Guid.Empty;
        externalUserId = _currentUser.UserId ?? Guid.Empty;

        return tenantId != Guid.Empty && externalUserId != Guid.Empty && _currentUser.IsAuthenticated;
    }
}
