using BuildingBlocks.Application.Extensions;
using Customer.Application.Customers.Commands.SyncCustomerFromIdentity;
using ECommerce.API.Contracts.Customer.Profile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Customer;

[Route("api/internal/customers")]
[ApiController]
public sealed class InternalCustomersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly string _serviceToken;

    public InternalCustomersController(ISender sender, IConfiguration configuration)
    {
        _sender = sender;
        _serviceToken = configuration["ServiceTokens:ECommerce"]!;
    }

    [HttpPost("{tenantId:int}/sync")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> SyncCustomer(
        [FromRoute] int tenantId,
        [FromBody] SyncCustomerRequest request,
        [FromHeader(Name = "Authorization")] string authHeader,
        CancellationToken cancellationToken)
    {
        var token = authHeader?.Replace("Bearer ", "");
        if (token != _serviceToken)
            return Unauthorized();

        var customerId = await _sender.Send(new SyncCustomerFromIdentityCommand(
            TenantIdConverter.ToGuid(tenantId),
            request.ExternalUserId,
            request.Email,
            request.FirstName,
            request.LastName), cancellationToken);

        return CreatedAtAction(nameof(SyncCustomer), new { tenantId, customerId }, new { CustomerId = customerId });
    }
}
