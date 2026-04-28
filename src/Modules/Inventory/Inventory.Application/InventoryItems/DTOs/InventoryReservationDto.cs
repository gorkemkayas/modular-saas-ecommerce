using Inventory.Domain.Enums;

namespace Inventory.Application.InventoryItems.DTOs;

public sealed record InventoryReservationDto(
    Guid Id,
    Guid OrderId,
    string ReservationReference,
    int Quantity,
    InventoryReservationStatus Status,
    DateTime CreatedAtUtc,
    DateTime? ReleasedAtUtc,
    DateTime? ConfirmedAtUtc);
