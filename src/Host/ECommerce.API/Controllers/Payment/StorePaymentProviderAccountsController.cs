using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using ECommerce.API.Contracts.Payment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.PaymentProviderAccounts.Commands.DisableIyzicoPaymentProviderAccount;
using Payment.Application.PaymentProviderAccounts.Commands.UpsertIyzicoPaymentProviderAccount;
using Payment.Application.PaymentProviderAccounts.DTOs;
using Payment.Application.PaymentProviderAccounts.Queries.GetIyzicoPaymentProviderAccount;

namespace ECommerce.API.Controllers.Payment;

[Route("api/stores/me/payment-provider-accounts")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
public sealed class StorePaymentProviderAccountsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public StorePaymentProviderAccountsController(ISender sender, ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet("iyzico")]
    [ProducesResponseType(typeof(IyzicoPaymentProviderAccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIyzico(CancellationToken cancellationToken)
    {
        var account = await _sender.Send(
            new GetIyzicoPaymentProviderAccountQuery(GetStoreId()),
            cancellationToken);

        return account is null ? NotFound() : Ok(account);
    }

    [HttpPut("iyzico")]
    [ProducesResponseType(typeof(IyzicoPaymentProviderAccountDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertIyzico(
        [FromBody] UpsertIyzicoPaymentProviderAccountRequest request,
        CancellationToken cancellationToken)
    {
        var account = await _sender.Send(
            new UpsertIyzicoPaymentProviderAccountCommand(
                GetStoreId(),
                request.ApiKey,
                request.SecretKey,
                request.IsEnabled),
            cancellationToken);

        return Ok(account);
    }

    [HttpPost("iyzico/disable")]
    [ProducesResponseType(typeof(IyzicoPaymentProviderAccountDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DisableIyzico(CancellationToken cancellationToken)
    {
        var account = await _sender.Send(
            new DisableIyzicoPaymentProviderAccountCommand(GetStoreId()),
            cancellationToken);

        return Ok(account);
    }

    private Guid GetStoreId() => _tenantContext.TenantIdAsGuid ?? Guid.Empty;
}
