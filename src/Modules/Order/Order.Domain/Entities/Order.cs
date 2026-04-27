using Order.Domain.Common;
using Order.Domain.Enums;
using Order.Domain.Exceptions;
using Order.Domain.Models;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public sealed class Order : IAggregateRoot
{
    private readonly List<OrderItem> _items = new();

    private Order()
    {
    }

    private Order(
        Guid id,
        Guid storeId,
        OrderNumber orderNumber,
        CustomerSnapshot customerSnapshot,
        OrderAddressSnapshot billingAddressSnapshot,
        OrderAddressSnapshot shippingAddressSnapshot,
        string currencyCode,
        IReadOnlyCollection<OrderItemDraft> items)
    {
        if (storeId == Guid.Empty)
            throw new OrderDomainException("Store id is required.");

        if (items.Count == 0)
            throw new OrderDomainException("Order must contain at least one item.");

        Id = id;
        StoreId = storeId;
        CustomerId = customerSnapshot.CustomerId;
        OrderNumber = orderNumber ?? throw new ArgumentNullException(nameof(orderNumber));
        CustomerSnapshot = customerSnapshot ?? throw new ArgumentNullException(nameof(customerSnapshot));
        BillingAddressSnapshot = billingAddressSnapshot ?? throw new ArgumentNullException(nameof(billingAddressSnapshot));
        ShippingAddressSnapshot = shippingAddressSnapshot ?? throw new ArgumentNullException(nameof(shippingAddressSnapshot));
        CurrencyCode = NormalizeCurrency(currencyCode);
        Status = OrderStatus.Confirmed;
        PaymentStatus = PaymentStatus.Pending;
        FulfillmentStatus = FulfillmentStatus.Unfulfilled;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
        PlacedAtUtc = CreatedAtUtc;

        foreach (var item in items)
        {
            if (item.UnitPriceSnapshot.CurrencyCode != CurrencyCode)
                throw new OrderDomainException("All order items must use the same currency as the order.");

            _items.Add(OrderItem.Create(
                id,
                item.ProductId,
                item.ProductVariantId,
                item.ProductName,
                item.VariantName,
                item.Sku,
                item.Quantity,
                item.UnitPriceSnapshot));
        }

        Totals = OrderTotals.FromItems(_items);
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderNumber OrderNumber { get; private set; } = default!;
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public FulfillmentStatus FulfillmentStatus { get; private set; }
    public string CurrencyCode { get; private set; } = default!;
    public CustomerSnapshot CustomerSnapshot { get; private set; } = default!;
    public OrderAddressSnapshot BillingAddressSnapshot { get; private set; } = default!;
    public OrderAddressSnapshot ShippingAddressSnapshot { get; private set; } = default!;
    public OrderTotals Totals { get; private set; } = default!;
    public DateTime PlacedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? ReservationReference { get; private set; }
    public string? PaymentReference { get; private set; }
    public string? ShipmentReference { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public static Order Place(
        Guid storeId,
        OrderNumber orderNumber,
        CustomerSnapshot customerSnapshot,
        OrderAddressSnapshot billingAddressSnapshot,
        OrderAddressSnapshot shippingAddressSnapshot,
        string currencyCode,
        IReadOnlyCollection<OrderItemDraft> items)
    {
        return new Order(
            Guid.NewGuid(),
            storeId,
            orderNumber,
            customerSnapshot,
            billingAddressSnapshot,
            shippingAddressSnapshot,
            currencyCode,
            items);
    }

    public void Cancel(string? reason)
    {
        if (Status == OrderStatus.Cancelled)
            return;

        if (Status == OrderStatus.Completed)
            throw new OrderDomainException("Completed order cannot be cancelled.");

        if (FulfillmentStatus is FulfillmentStatus.Shipped or FulfillmentStatus.Delivered)
            throw new OrderDomainException("Shipped or delivered order cannot be cancelled.");

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        CancellationReason = NormalizeOptional(reason, 500);
        UpdatedAtUtc = CancelledAtUtc.Value;
    }

    public void MarkPaymentAuthorized(string? paymentReference = null)
    {
        EnsureNotCancelled();
        PaymentStatus = PaymentStatus.Authorized;
        PaymentReference = NormalizeOptional(paymentReference, 200);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkPaymentCaptured(string? paymentReference = null)
    {
        EnsureNotCancelled();
        PaymentStatus = PaymentStatus.Captured;
        PaymentReference = NormalizeOptional(paymentReference, 200);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkPaymentFailed(string? paymentReference = null)
    {
        EnsureNotCancelled();
        PaymentStatus = PaymentStatus.Failed;
        PaymentReference = NormalizeOptional(paymentReference, 200);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkShipmentCreated(string shipmentReference)
    {
        EnsureNotCancelled();
        ShipmentReference = NormalizeRequired(shipmentReference, "Shipment reference");
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkShipped(string shipmentReference)
    {
        EnsureNotCancelled();
        ShipmentReference = NormalizeRequired(shipmentReference, "Shipment reference");
        FulfillmentStatus = FulfillmentStatus.Shipped;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkDelivered(string shipmentReference)
    {
        EnsureNotCancelled();
        ShipmentReference = NormalizeRequired(shipmentReference, "Shipment reference");
        FulfillmentStatus = FulfillmentStatus.Delivered;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Complete()
    {
        EnsureNotCancelled();
        Status = OrderStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetReservationReference(string reservationReference)
    {
        EnsureNotCancelled();
        ReservationReference = NormalizeRequired(reservationReference, "Reservation reference");
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void EnsureNotCancelled()
    {
        if (Status == OrderStatus.Cancelled)
            throw new OrderDomainException("Cancelled order cannot be modified.");
    }

    private static string NormalizeCurrency(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new OrderDomainException("Currency code is required.");

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != 3)
            throw new OrderDomainException("Currency code must be 3 characters.");

        return normalized;
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new OrderDomainException($"{fieldName} is required.");

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new OrderDomainException("Value is too long.");

        return normalized;
    }
}
