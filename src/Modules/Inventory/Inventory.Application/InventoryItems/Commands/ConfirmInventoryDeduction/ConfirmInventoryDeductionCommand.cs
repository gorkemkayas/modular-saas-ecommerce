using MediatR;

namespace Inventory.Application.InventoryItems.Commands.ConfirmInventoryDeduction;

public sealed record ConfirmInventoryDeductionCommand(
    Guid StoreId,
    string ReservationReference,
    string Reason) : IRequest;
