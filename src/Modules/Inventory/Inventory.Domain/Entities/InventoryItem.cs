using Inventory.Domain.Common;
using Inventory.Domain.Enums;
using Inventory.Domain.Exceptions;

namespace Inventory.Domain.Entities;

public sealed class InventoryItem : IAggregateRoot
{
    private readonly List<InventoryReservation> _reservations = new();
    private readonly List<StockMovement> _movements = new();

    private InventoryItem()
    {
    }

    private InventoryItem(
        Guid id,
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        string sku,
        string displayName,
        int initialOnHandQuantity,
        int? reorderThreshold)
    {
        if (storeId == Guid.Empty)
            throw new InventoryDomainException("Store id is required.");

        if (productId == Guid.Empty)
            throw new InventoryDomainException("Product id is required.");

        if (initialOnHandQuantity < 0)
            throw new InventoryDomainException("Initial on-hand quantity cannot be negative.");

        if (reorderThreshold.HasValue && reorderThreshold.Value < 0)
            throw new InventoryDomainException("Reorder threshold cannot be negative.");

        Id = id;
        StoreId = storeId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        SellableItemKey = CreateSellableItemKey(productId, productVariantId);
        Sku = NormalizeRequired(sku, "Sku", 100);
        DisplayName = NormalizeRequired(displayName, "Display name", 250);
        OnHandQuantity = initialOnHandQuantity;
        ReservedQuantity = 0;
        ReorderThreshold = reorderThreshold;
        Version = 1;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;

        _movements.Add(StockMovement.Create(
            Id,
            StockMovementType.Created,
            initialOnHandQuantity,
            0,
            OnHandQuantity,
            ReservedQuantity,
            "Inventory item created."));
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public string SellableItemKey { get; private set; } = default!;
    public string Sku { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public int OnHandQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => OnHandQuantity - ReservedQuantity;
    public int? ReorderThreshold { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public bool IsLowStock => ReorderThreshold.HasValue && AvailableQuantity <= ReorderThreshold.Value;

    public IReadOnlyCollection<InventoryReservation> Reservations => _reservations.AsReadOnly();
    public IReadOnlyCollection<StockMovement> Movements => _movements.AsReadOnly();

    public static InventoryItem Create(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        string sku,
        string displayName,
        int initialOnHandQuantity,
        int? reorderThreshold)
    {
        return new InventoryItem(
            Guid.NewGuid(),
            storeId,
            productId,
            productVariantId,
            sku,
            displayName,
            initialOnHandQuantity,
            reorderThreshold);
    }

    public void AddStock(int quantity, string reason, string? reference = null)
    {
        if (quantity <= 0)
            throw new InventoryDomainException("Quantity to add must be greater than zero.");

        OnHandQuantity += quantity;
        RecordMovement(StockMovementType.StockAdded, quantity, 0, reason, reference);
    }

    public void AdjustStock(int newOnHandQuantity, string reason, string? reference = null)
    {
        if (newOnHandQuantity < 0)
            throw new InventoryDomainException("On-hand quantity cannot be negative.");

        if (newOnHandQuantity < ReservedQuantity)
            throw new InventoryDomainException("On-hand quantity cannot be less than reserved quantity.");

        var delta = newOnHandQuantity - OnHandQuantity;
        OnHandQuantity = newOnHandQuantity;
        RecordMovement(StockMovementType.StockAdjusted, delta, 0, reason, reference);
    }

    public void SetReorderThreshold(int? reorderThreshold)
    {
        if (reorderThreshold.HasValue && reorderThreshold.Value < 0)
            throw new InventoryDomainException("Reorder threshold cannot be negative.");

        ReorderThreshold = reorderThreshold;
        RecordMovement(
            StockMovementType.ReorderThresholdChanged,
            0,
            0,
            reorderThreshold.HasValue
                ? $"Reorder threshold set to {reorderThreshold.Value}."
                : "Reorder threshold cleared.");
    }

    public void EnsureAvailability(int quantity)
    {
        if (quantity <= 0)
            throw new InventoryDomainException("Requested quantity must be greater than zero.");

        if (AvailableQuantity < quantity)
            throw new InventoryDomainException("Insufficient available stock.");
    }

    public InventoryReservation Reserve(
        Guid orderId,
        string reservationReference,
        int quantity)
    {
        if (quantity <= 0)
            throw new InventoryDomainException("Reservation quantity must be greater than zero.");

        var existingReservation = FindReservation(reservationReference);

        if (existingReservation is not null)
        {
            if (existingReservation.Status != InventoryReservationStatus.Active)
                throw new InventoryDomainException("Reservation reference already exists in a terminal state.");

            if (existingReservation.OrderId != orderId || existingReservation.Quantity != quantity)
                throw new InventoryDomainException("Reservation reference already exists with different data.");

            return existingReservation;
        }

        EnsureAvailability(quantity);

        var reservation = InventoryReservation.Create(Id, orderId, reservationReference, quantity);
        _reservations.Add(reservation);
        ReservedQuantity += quantity;

        RecordMovement(
            StockMovementType.Reserved,
            0,
            quantity,
            $"Reserved {quantity} unit(s).",
            reservationReference);

        return reservation;
    }

    public void ReleaseReservation(string reservationReference, string reason)
    {
        var reservation = FindReservation(reservationReference)
            ?? throw new InventoryDomainException("Reservation was not found.");

        if (reservation.Status == InventoryReservationStatus.Released)
            return;

        reservation.Release();
        ReservedQuantity -= reservation.Quantity;

        RecordMovement(
            StockMovementType.ReservationReleased,
            0,
            -reservation.Quantity,
            reason,
            reservationReference);
    }

    public void ConfirmReservation(string reservationReference, string reason)
    {
        var reservation = FindReservation(reservationReference)
            ?? throw new InventoryDomainException("Reservation was not found.");

        if (reservation.Status == InventoryReservationStatus.Confirmed)
            return;

        reservation.Confirm();
        ReservedQuantity -= reservation.Quantity;
        OnHandQuantity -= reservation.Quantity;

        if (OnHandQuantity < 0)
            throw new InventoryDomainException("On-hand quantity cannot become negative.");

        RecordMovement(
            StockMovementType.Deducted,
            -reservation.Quantity,
            -reservation.Quantity,
            reason,
            reservationReference);
    }

    private InventoryReservation? FindReservation(string reservationReference)
    {
        var normalizedReference = NormalizeRequired(reservationReference, "Reservation reference", 100);

        return _reservations.FirstOrDefault(x => x.ReservationReference == normalizedReference);
    }

    private void RecordMovement(
        StockMovementType type,
        int onHandDelta,
        int reservedDelta,
        string reason,
        string? reference = null)
    {
        UpdatedAtUtc = DateTime.UtcNow;
        Version++;

        _movements.Add(StockMovement.Create(
            Id,
            type,
            onHandDelta,
            reservedDelta,
            OnHandQuantity,
            ReservedQuantity,
            reason,
            reference));
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

    public static string CreateSellableItemKey(Guid productId, Guid? productVariantId)
    {
        if (productId == Guid.Empty)
            throw new InventoryDomainException("Product id is required.");

        return productVariantId.HasValue
            ? $"{productId:N}:{productVariantId.Value:N}"
            : $"{productId:N}:simple";
    }
}
