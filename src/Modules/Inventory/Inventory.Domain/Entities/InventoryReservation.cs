using Inventory.Domain.Enums;
using Inventory.Domain.Exceptions;

namespace Inventory.Domain.Entities;

public sealed class InventoryReservation
{
    private InventoryReservation()
    {
    }

    private InventoryReservation(
        Guid id,
        Guid inventoryItemId,
        Guid orderId,
        string reservationReference,
        int quantity)
    {
        if (inventoryItemId == Guid.Empty)
            throw new InventoryDomainException("Inventory item id is required.");

        if (orderId == Guid.Empty)
            throw new InventoryDomainException("Order id is required.");

        if (quantity <= 0)
            throw new InventoryDomainException("Reservation quantity must be greater than zero.");

        Id = id;
        InventoryItemId = inventoryItemId;
        OrderId = orderId;
        ReservationReference = NormalizeRequired(reservationReference, "Reservation reference", 100);
        Quantity = quantity;
        Status = InventoryReservationStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid OrderId { get; private set; }
    public string ReservationReference { get; private set; } = default!;
    public int Quantity { get; private set; }
    public InventoryReservationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReleasedAtUtc { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }

    public static InventoryReservation Create(
        Guid inventoryItemId,
        Guid orderId,
        string reservationReference,
        int quantity)
    {
        return new InventoryReservation(
            Guid.NewGuid(),
            inventoryItemId,
            orderId,
            reservationReference,
            quantity);
    }

    public void Release()
    {
        if (Status == InventoryReservationStatus.Released)
            return;

        if (Status == InventoryReservationStatus.Confirmed)
            throw new InventoryDomainException("Confirmed reservation cannot be released.");

        Status = InventoryReservationStatus.Released;
        ReleasedAtUtc = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status == InventoryReservationStatus.Confirmed)
            return;

        if (Status == InventoryReservationStatus.Released)
            throw new InventoryDomainException("Released reservation cannot be confirmed.");

        Status = InventoryReservationStatus.Confirmed;
        ConfirmedAtUtc = DateTime.UtcNow;
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InventoryDomainException($"{fieldName} is required.");

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new InventoryDomainException($"{fieldName} cannot exceed {maxLength} characters.");

        return normalized;
    }
}
