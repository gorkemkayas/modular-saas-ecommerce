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
                "Your order is confirmed — {{OrderNumber}}",
                """
                Dear {{RecipientName}},

                Thank you for your order. We are delighted to confirm that order {{OrderNumber}} has been received and is now being prepared with care.

                A summary of your purchase is below. We will be in touch again as soon as it is on its way.
                """),
            new(
                NotificationTrigger.OrderCancelled,
                "Default Order Cancelled Email",
                "Your order has been cancelled — {{OrderNumber}}",
                """
                Dear {{RecipientName}},

                We are writing to let you know that your order {{OrderNumber}} has been cancelled.

                If this was not expected, our team is always happy to help — simply reply to this email.
                """),
            new(
                NotificationTrigger.PaymentAuthorized,
                "Default Payment Authorized Email",
                "Your payment is authorized — {{OrderNumber}}",
                """
                Dear {{RecipientName}},

                The payment for your order {{OrderNumber}} has been successfully authorized. There is nothing further you need to do.
                """),
            new(
                NotificationTrigger.PaymentCaptured,
                "Default Payment Captured Email",
                "Your payment is complete — {{OrderNumber}}",
                """
                Dear {{RecipientName}},

                We are pleased to confirm that the payment for your order {{OrderNumber}} has been completed. Thank you for your purchase.
                """),
            new(
                NotificationTrigger.PaymentFailed,
                "Default Payment Failed Email",
                "We could not process your payment — {{OrderNumber}}",
                """
                Dear {{RecipientName}},

                Unfortunately, we were unable to process the payment for your order {{OrderNumber}}. No charge has been made.

                Please try again, or reply to this email if you would like assistance.
                """),
            new(
                NotificationTrigger.PaymentRefunded,
                "Default Payment Refunded Email",
                "Your refund is complete — {{OrderNumber}}",
                """
                Dear {{RecipientName}},

                Your refund for order {{OrderNumber}} has been processed. Please allow a few business days for it to appear on your statement.
                """),
            new(
                NotificationTrigger.ShipmentCreated,
                "Default Shipment Created Email",
                "Your order is being prepared — {{OrderNumber}}",
                """
                Dear {{RecipientName}},

                Good news — a shipment has been prepared for your order {{OrderNumber}}. The details are below, and we will notify you the moment it is dispatched.
                """),
            new(
                NotificationTrigger.ShipmentShipped,
                "Default Shipment Shipped Email",
                "Your order is on its way — {{OrderNumber}}",
                """
                Dear {{RecipientName}},

                Your order {{OrderNumber}} is now on its way to you. You can follow its journey using the tracking details below.
                """),
            new(
                NotificationTrigger.ShipmentDelivered,
                "Default Shipment Delivered Email",
                "Your order has arrived — {{OrderNumber}}",
                """
                Dear {{RecipientName}},

                Your order {{OrderNumber}} has been delivered. We hope you love it — thank you for shopping with us.
                """),
            new(
                NotificationTrigger.ShipmentDeliveryException,
                "Default Delivery Update Email",
                "An update on your delivery — {{OrderNumber}}",
                """
                Dear {{RecipientName}},

                We have an update regarding the delivery of your order {{OrderNumber}}. Please find the latest details below.
                """)
        ];
    }

    private sealed record NotificationTemplateDefinition(
        NotificationTrigger Trigger,
        string Name,
        string SubjectTemplate,
        string BodyTemplate);
}
