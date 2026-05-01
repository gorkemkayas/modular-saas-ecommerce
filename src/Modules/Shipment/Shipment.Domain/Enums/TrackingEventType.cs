namespace Shipment.Domain.Enums;

public enum TrackingEventType
{
    Created = 0,
    LabelCreated = 1,
    PickedUp = 2,
    InTransit = 3,
    OutForDelivery = 4,
    Delivered = 5,
    DeliveryAttemptFailed = 6,
    Exception = 7,
    ReturnedToSender = 8
}
