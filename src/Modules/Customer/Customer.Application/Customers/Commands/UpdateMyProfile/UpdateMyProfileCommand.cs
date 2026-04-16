using MediatR;

namespace Customer.Application.Customers.Commands.UpdateMyProfile;

public sealed record UpdateMyProfileCommand(
    Guid TenantId,
    Guid ExternalUserId,
    string FirstName,
    string LastName,
    string? PhoneNumber) : IRequest;
