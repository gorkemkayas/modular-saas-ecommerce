using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using Customer.Application.Common.Models;
using Customer.Application.Customers.Commands.ActivateCustomer;
using Customer.Application.Customers.Commands.BlockCustomer;
using Customer.Application.Customers.DTOs;
using Customer.Application.Customers.Queries.GetCustomerById;
using Customer.Application.Customers.Queries.SearchCustomers;
using Customer.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Customer;

[Route("api/customers")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
public sealed class CustomersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public CustomersController(ISender sender, ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] string? searchTerm,
        [FromQuery] CustomerStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new SearchCustomersQuery(
            GetTenantId(),
            searchTerm,
            status,
            pageNumber,
            pageSize), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{customerId:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerById([FromRoute] Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _sender.Send(new GetCustomerByIdQuery(GetTenantId(), customerId), cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost("{customerId:guid}/block")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> BlockCustomer([FromRoute] Guid customerId, CancellationToken cancellationToken)
    {
        await _sender.Send(new BlockCustomerCommand(GetTenantId(), customerId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{customerId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActivateCustomer([FromRoute] Guid customerId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateCustomerCommand(GetTenantId(), customerId), cancellationToken);
        return NoContent();
    }

    private Guid GetTenantId() => _tenantContext.TenantIdAsGuid ?? Guid.Empty;
}
