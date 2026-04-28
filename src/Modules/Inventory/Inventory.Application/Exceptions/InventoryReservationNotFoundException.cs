namespace Inventory.Application.Exceptions;

public sealed class InventoryReservationNotFoundException : ApplicationException
{
    public InventoryReservationNotFoundException(string reservationReference)
        : base($"Inventory reservation '{reservationReference}' was not found.")
    {
        ReservationReference = reservationReference;
    }

    public string ReservationReference { get; }
}
