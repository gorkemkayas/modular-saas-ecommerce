using Customer.Domain.Enums;
using MediatR;

namespace Customer.Application.Customers.Commands.UpsertConsent;

public sealed record UpsertConsentCommand(
    Guid TenantId,
    Guid ExternalUserId,
    ConsentType ConsentType,
    bool IsGranted,
    string Source) : IRequest;
