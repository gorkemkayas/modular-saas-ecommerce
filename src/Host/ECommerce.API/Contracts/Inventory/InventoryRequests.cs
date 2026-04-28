namespace ECommerce.API.Contracts.Inventory;

public sealed record SearchInventoryItemsRequest(
    Guid? ProductId,
    Guid? ProductVariantId,
    bool OnlyLowStock = false,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 20);

public sealed record CreateInventoryItemRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    int InitialOnHandQuantity,
    int? ReorderThreshold);

public sealed record AddStockRequest(
    int Quantity,
    string Reason,
    string? Reference);

public sealed record AdjustStockRequest(
    int NewOnHandQuantity,
    string Reason,
    string? Reference);

public sealed record SetReorderThresholdRequest(int? ReorderThreshold);

public sealed record ReserveInventoryRequest(
    Guid OrderId,
    string ReservationReference,
    IReadOnlyCollection<ReserveInventoryItemRequest> Items);

public sealed record ReserveInventoryItemRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    int Quantity);

public sealed record ReleaseInventoryReservationRequest(string Reason);

public sealed record ConfirmInventoryDeductionRequest(string Reason);

public sealed record GetInventoryMovementsRequest(
    int PageNumber = 1,
    int PageSize = 50);
