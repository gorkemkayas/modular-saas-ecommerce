using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Seeding;

public sealed class NotificationTemplateSeeder
{
    private static readonly Guid DefaultStoreId = Guid.Empty;
    private const string DefaultLocale = "default";

    private readonly NotificationDbContext _context;
    private readonly ILogger<NotificationTemplateSeeder> _logger;

    public NotificationTemplateSeeder(
        NotificationDbContext context,
        ILogger<NotificationTemplateSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var definition in GetDefinitions())
        {
            var exists = await _context.NotificationTemplates.AnyAsync(
                x => x.StoreId == DefaultStoreId
                    && x.Trigger == definition.Trigger
                    && x.Channel == NotificationChannel.Email
                    && x.Locale == DefaultLocale,
                cancellationToken);

            if (exists)
                continue;

            var template = NotificationTemplate.Create(
                DefaultStoreId,
                definition.Name,
                definition.Trigger,
                NotificationChannel.Email,
                DefaultLocale,
                definition.SubjectTemplate,
                definition.BodyTemplate);

            await _context.NotificationTemplates.AddAsync(template, cancellationToken);

            _logger.LogInformation(
                "Seeded default notification template | Trigger: {Trigger} | Channel: {Channel}",
                definition.Trigger,
                NotificationChannel.Email);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyCollection<NotificationTemplateDefinition> GetDefinitions()
    {
        return
        [
            new(
                NotificationTrigger.OrderPlaced,
                "Default Order Placed Email",
                "Your order {{OrderNumber}} has been received",
                """
                Hi {{RecipientName}},

                We received your order {{OrderNumber}} successfully.

                Order total: {{GrandTotalAmount}} {{CurrencyCode}}

                We will notify you again when your order moves forward.
                """),
            new(
                NotificationTrigger.OrderCancelled,
                "Default Order Cancelled Email",
                "Your order {{OrderNumber}} has been cancelled",
                """
                Hi {{RecipientName}},

                Your order {{OrderNumber}} has been cancelled.

                Reason: {{CancellationReason}}
                """),
            new(
                NotificationTrigger.PaymentAuthorized,
                "Default Payment Authorized Email",
                "Payment authorized for order {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                Your payment for order {{OrderNumber}} has been authorized.

                Amount: {{PaymentAmount}} {{CurrencyCode}}
                Reference: {{PaymentReference}}
                """),
            new(
                NotificationTrigger.PaymentCaptured,
                "Default Payment Captured Email",
                "Payment captured for order {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                Your payment for order {{OrderNumber}} has been captured successfully.

                Amount: {{PaymentAmount}} {{CurrencyCode}}
                Reference: {{PaymentReference}}
                """),
            new(
                NotificationTrigger.PaymentFailed,
                "Default Payment Failed Email",
                "Payment failed for order {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                Your payment for order {{OrderNumber}} could not be completed.

                Amount: {{PaymentAmount}} {{CurrencyCode}}
                Details: {{FailureMessage}}
                """),
            new(
                NotificationTrigger.PaymentRefunded,
                "Default Payment Refunded Email",
                "Refund completed for order {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                A refund has been completed for order {{OrderNumber}}.

                Refunded amount: {{RefundedAmount}} {{CurrencyCode}}
                """),
            new(
                NotificationTrigger.ShipmentCreated,
                "Default Shipment Created Email",
                "Shipment created for order {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                A shipment has been created for your order {{OrderNumber}}.

                Shipment number: {{ShipmentNumber}}
                Carrier: {{CarrierName}}
                Tracking number: {{TrackingNumber}}
                Tracking URL: {{TrackingUrl}}
                """),
            new(
                NotificationTrigger.ShipmentShipped,
                "Default Shipment Shipped Email",
                "Your order {{OrderNumber}} is on the way",
                """
                Hi {{RecipientName}},

                Your order {{OrderNumber}} has been shipped.

                Shipment number: {{ShipmentNumber}}
                Carrier: {{CarrierName}}
                Tracking number: {{TrackingNumber}}
                Tracking URL: {{TrackingUrl}}
                """),
            new(
                NotificationTrigger.ShipmentDelivered,
                "Default Shipment Delivered Email",
                "Your order {{OrderNumber}} has been delivered",
                """
                Hi {{RecipientName}},

                Your order {{OrderNumber}} has been delivered.

                Shipment number: {{ShipmentNumber}}
                Carrier: {{CarrierName}}
                """),
            new(
                NotificationTrigger.ShipmentDeliveryException,
                "Default Shipment Exception Email",
                "Delivery update for order {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                There is a delivery update for your order {{OrderNumber}}.

                Shipment number: {{ShipmentNumber}}
                Carrier: {{CarrierName}}
                Tracking number: {{TrackingNumber}}
                Details: {{Description}}
                """)
        ];
    }

    private sealed record NotificationTemplateDefinition(
        NotificationTrigger Trigger,
        string Name,
        string SubjectTemplate,
        string BodyTemplate);
}
