using BuildingBlocks.Domain.Common;
using Order.Domain.Common;
using Order.Domain.Enums;
using Order.Domain.Events;
using Order.Domain.Exceptions;
using Order.Domain.Models;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public sealed class Order : AggregateRoot, IAggregateRoot
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
        IReadOnlyCollection<OrderItemDraft> items,
        ShippingCarrierSnapshot? shippingCarrierSnapshot)
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
        ShippingCarrierSnapshot = shippingCarrierSnapshot;
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
                item.UnitPriceSnapshot,
                item.ImageUrl));
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
    public ShippingCarrierSnapshot? ShippingCarrierSnapshot { get; private set; }
    public OrderTotals Totals { get; private set; } = default!;
    public DateTime PlacedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? ReservationReference { get; private set; }
    public string? PaymentReference { get; private set; }
    public string? ShipmentReference { get; private set; }
    public Guid? LastFulfillmentShipmentId { get; private set; }
    public long LastFulfillmentRevision { get; private set; }
    public ShipmentFulfillmentState? LastShipmentFulfillmentState { get; private set; }
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
        IReadOnlyCollection<OrderItemDraft> items,
        ShippingCarrierSnapshot? shippingCarrierSnapshot = null)
    {
        return new Order(
            Guid.NewGuid(),
            storeId,
            orderNumber,
            customerSnapshot,
            billingAddressSnapshot,
            shippingAddressSnapshot,
            currencyCode,
            items,
            shippingCarrierSnapshot);
    }

    public void Cancel(string? reason)
    {
        if (Status == OrderStatus.Cancelled)
            return;

        if (Status == OrderStatus.Completed)
            throw new OrderDomainException("Completed order cannot be cancelled.");

        if (PaymentStatus is PaymentStatus.Authorized or PaymentStatus.Captured or PaymentStatus.Refunded)
            throw new OrderDomainException("Authorized payment must be cancelled first; captured or refunded orders cannot be cancelled.");

        if (FulfillmentStatus is FulfillmentStatus.Shipped or FulfillmentStatus.Delivered)
            throw new OrderDomainException("Shipped or delivered order cannot be cancelled.");

        var occurredOnUtc = DateTime.UtcNow;

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = occurredOnUtc;
        CancellationReason = NormalizeOptional(reason, 500);
        UpdatedAtUtc = occurredOnUtc;

        RaiseDomainEvent(new OrderCancelledDomainEvent(
            Guid.NewGuid(),
            occurredOnUtc,
            Id,
            StoreId,
            CustomerId,
            OrderNumber.Value,
            CustomerSnapshot.Email,
            CustomerSnapshot.FullName,
            CancellationReason));
        if (!string.IsNullOrWhiteSpace(ReservationReference))
            RaiseDomainEvent(new OrderReservationCancelledDomainEvent(Guid.NewGuid(), occurredOnUtc,
                StoreId, Id, ReservationReference, CancellationReason));
    }

    private void RaiseOrderPlacedEvent()
    {
        RaiseDomainEvent(new OrderPlacedDomainEvent(
            Guid.NewGuid(), DateTime.UtcNow, Id, StoreId, CustomerId, OrderNumber.Value,
            CustomerSnapshot.Email, CustomerSnapshot.FullName, CurrencyCode,
            Totals.SubtotalAmount, Totals.ShippingAmount, Totals.GrandTotalAmount,
            Items.Select(x => new OrderPlacedLine(x.ProductId, x.ProductVariantId,
                x.ProductName, x.VariantName, x.Quantity, x.LineTotalAmount, x.ImageUrl)).ToArray()));
    }

    public void MarkPaymentAuthorized(string? paymentReference = null)
    {
        EnsureNotCancelled();
        var notify = PaymentStatus is not PaymentStatus.Authorized and not PaymentStatus.Captured;
        PaymentStatus = PaymentStatus.Authorized;
        PaymentReference = NormalizeOptional(paymentReference, 200);
        UpdatedAtUtc = DateTime.UtcNow;
        if (notify) RaiseOrderPlacedEvent();
    }

    public void MarkPaymentCaptured(string? paymentReference = null)
    {
        EnsureNotCancelled();
        var notify = PaymentStatus is not PaymentStatus.Authorized and not PaymentStatus.Captured;
        PaymentStatus = PaymentStatus.Captured;
        PaymentReference = NormalizeOptional(paymentReference, 200);
        UpdatedAtUtc = DateTime.UtcNow;
        if (notify) RaiseOrderPlacedEvent();
    }

    public void MarkPaymentFailed(string? paymentReference = null)
    {
        EnsureNotCancelled();
        PaymentStatus = PaymentStatus.Failed;
        PaymentReference = NormalizeOptional(paymentReference, 200);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkPaymentRefunded(string? paymentReference = null)
    {
        EnsureNotCancelled();
        PaymentStatus = PaymentStatus.Refunded;
        PaymentReference = NormalizeOptional(paymentReference, 200);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool ApplyShipmentFulfillment(Guid shipmentId, string shipmentNumber,
        ShipmentFulfillmentState shipmentState, long revision)
    {
        if (shipmentId == Guid.Empty || revision <= 0 || !Enum.IsDefined(shipmentState))
            throw new OrderDomainException("Shipment identity, positive revision and known status are required.");
        string reference = NormalizeRequired(shipmentNumber, "Shipment number");
        if (reference.Length > 200)
            throw new OrderDomainException("Shipment number cannot exceed 200 characters.");
        if (revision <= LastFulfillmentRevision)
            return false;

        EnsureNotCancelled();
        if (Status == OrderStatus.Completed && shipmentState != ShipmentFulfillmentState.Delivered)
            throw new OrderDomainException("Completed order fulfillment cannot move to an earlier state.");
        if (FulfillmentStatus == FulfillmentStatus.Returned)
            throw new OrderDomainException("Returned order fulfillment cannot be overwritten by shipment updates.");

        bool sameShipment = LastFulfillmentShipmentId == shipmentId;
        if (sameShipment && (LastShipmentFulfillmentState is
            ShipmentFulfillmentState.Delivered or ShipmentFulfillmentState.Cancelled) &&
            LastShipmentFulfillmentState != shipmentState)
            throw new OrderDomainException("A terminal shipment cannot change its fulfillment state.");

        if (FulfillmentStatus is FulfillmentStatus.Shipped or FulfillmentStatus.Delivered)
        {
            if (LastFulfillmentShipmentId.HasValue && !sameShipment)
                throw new OrderDomainException("A shipped order cannot switch to a replacement shipment.");
            if (shipmentState is ShipmentFulfillmentState.Draft or ShipmentFulfillmentState.ReadyForDispatch or ShipmentFulfillmentState.Cancelled)
                throw new OrderDomainException("Shipped order fulfillment cannot move back to unfulfilled.");
            if (FulfillmentStatus == FulfillmentStatus.Delivered && shipmentState != ShipmentFulfillmentState.Delivered)
                throw new OrderDomainException("Delivered order fulfillment cannot move backwards.");
        }

        if (sameShipment && ShipmentReference is not null &&
            !string.Equals(ShipmentReference, reference, StringComparison.Ordinal))
            throw new OrderDomainException("Shipment number cannot change for the same shipment.");

        FulfillmentStatus = shipmentState switch
        {
            ShipmentFulfillmentState.Shipped or ShipmentFulfillmentState.DeliveryException => FulfillmentStatus.Shipped,
            ShipmentFulfillmentState.Delivered => FulfillmentStatus.Delivered,
            _ => FulfillmentStatus.Unfulfilled
        };
        ShipmentReference = shipmentState == ShipmentFulfillmentState.Cancelled ? null : reference;
        LastFulfillmentShipmentId = shipmentId;
        LastShipmentFulfillmentState = shipmentState;
        LastFulfillmentRevision = revision;
        UpdatedAtUtc = DateTime.UtcNow;
        return true;
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
