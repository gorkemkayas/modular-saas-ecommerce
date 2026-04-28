using MediatR;

namespace Inventory.Application.InventoryItems.Commands.ReleaseInventoryReservation;

public sealed record ReleaseInventoryReservationCommand(
    Guid StoreId,
    string ReservationReference,
    string Reason) : IRequest;
