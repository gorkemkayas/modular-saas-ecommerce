using Customer.Application.Abstractions.Queries;
using Customer.Application.Common.Models;
using Customer.Application.Customers.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Customer.Infrastructure.ReadServices;

public sealed class CustomerReadService : ICustomerReadService
{
    private readonly Persistence.CustomerDbContext _context;

    public CustomerReadService(Persistence.CustomerDbContext context)
    {
        _context = context;
    }

    public Task<CustomerDto?> GetByIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return BuildCustomerQuery(tenantId)
            .FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);
    }

    public Task<CustomerDto?> GetByExternalUserIdAsync(Guid tenantId, Guid externalUserId, CancellationToken cancellationToken = default)
    {
        return BuildCustomerQuery(tenantId)
            .FirstOrDefaultAsync(x => x.ExternalUserId == externalUserId, cancellationToken);
    }

    public async Task<PagedResult<CustomerSummaryDto>> SearchAsync(CustomerSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers
            .AsNoTracking()
            .Where(x => x.TenantId == criteria.TenantId);

        if (criteria.Status.HasValue)
            query = query.Where(x => x.Status == criteria.Status.Value);

        var normalizedSearch = Normalize(criteria.SearchTerm);

        if (normalizedSearch is not null)
        {
            query = query.Where(x =>
                x.Email.Value.Contains(normalizedSearch)
                || x.Name.FirstName.ToLower().Contains(normalizedSearch)
                || x.Name.LastName.ToLower().Contains(normalizedSearch));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ThenBy(x => x.Name.FirstName)
            .ThenBy(x => x.Name.LastName)
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(x => new CustomerSummaryDto(
                x.Id,
                x.ExternalUserId,
                x.Email.Value,
                x.Name.FirstName + " " + x.Name.LastName,
                x.PhoneNumber == null ? null : x.PhoneNumber.Value,
                x.Status,
                x.Addresses.Count,
                x.RegisteredAtUtc,
                x.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<CustomerSummaryDto>(
            items,
            criteria.PageNumber,
            criteria.PageSize,
            totalCount);
    }

    private IQueryable<CustomerDto> BuildCustomerQuery(Guid tenantId)
    {
        return _context.Customers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new CustomerDto(
                x.Id,
                x.TenantId,
                x.ExternalUserId,
                x.Email.Value,
                x.Name.FirstName,
                x.Name.LastName,
                x.PhoneNumber == null ? null : x.PhoneNumber.Value,
                x.Status,
                x.RegisteredAtUtc,
                x.UpdatedAtUtc,
                new CustomerPreferencesDto(
                    x.PreferredLanguage,
                    x.PreferredCurrency),
                x.Addresses
                    .OrderByDescending(address => address.IsDefaultShipping)
                    .ThenByDescending(address => address.IsDefaultBilling)
                    .ThenBy(address => address.Title)
                    .Select(address => new CustomerAddressDto(
                        address.Id,
                        address.AddressType,
                        address.Title,
                        address.ContactName,
                        address.PhoneNumber.Value,
                        address.Country,
                        address.City,
                        address.District,
                        address.Line1,
                        address.Line2,
                        address.PostalCode,
                        address.IsDefaultShipping,
                        address.IsDefaultBilling))
                    .ToArray(),
                x.Consents
                    .OrderBy(consent => consent.ConsentType)
                    .Select(consent => new CustomerConsentDto(
                        consent.ConsentType,
                        consent.IsGranted,
                        consent.Source,
                        consent.UpdatedAtUtc))
                    .ToArray()));
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToLowerInvariant();
    }
}
