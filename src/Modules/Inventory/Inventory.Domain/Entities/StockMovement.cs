using Inventory.Domain.Enums;
using Inventory.Domain.Exceptions;

namespace Inventory.Domain.Entities;

public sealed class StockMovement
{
    private StockMovement()
    {
    }

    private StockMovement(
        Guid id,
        Guid inventoryItemId,
        StockMovementType type,
        int onHandDelta,
        int reservedDelta,
        int resultingOnHandQuantity,
        int resultingReservedQuantity,
        string reason,
        string? reference)
    {
        if (inventoryItemId == Guid.Empty)
            throw new InventoryDomainException("Inventory item id is required.");

        Id = id;
        InventoryItemId = inventoryItemId;
        Type = type;
        OnHandDelta = onHandDelta;
        ReservedDelta = reservedDelta;
        ResultingOnHandQuantity = resultingOnHandQuantity;
        ResultingReservedQuantity = resultingReservedQuantity;
        Reason = NormalizeRequired(reason, "Reason", 250);
        Reference = NormalizeOptional(reference, 100);
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public StockMovementType Type { get; private set; }
    public int OnHandDelta { get; private set; }
    public int ReservedDelta { get; private set; }
    public int ResultingOnHandQuantity { get; private set; }
    public int ResultingReservedQuantity { get; private set; }
    public string Reason { get; private set; } = default!;
    public string? Reference { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static StockMovement Create(
        Guid inventoryItemId,
        StockMovementType type,
        int onHandDelta,
        int reservedDelta,
        int resultingOnHandQuantity,
        int resultingReservedQuantity,
        string reason,
        string? reference = null)
    {
        return new StockMovement(
            Guid.NewGuid(),
            inventoryItemId,
            type,
            onHandDelta,
            reservedDelta,
            resultingOnHandQuantity,
            resultingReservedQuantity,
            reason,
            reference);
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

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new InventoryDomainException($"Reference cannot exceed {maxLength} characters.");

        return normalized;
    }
}
