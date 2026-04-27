namespace Order.Application.Orders.DTOs;

public sealed record OrderCustomerSnapshotDto(
    Guid CustomerId,
    string Email,
    string FullName,
    string? PhoneNumber);
