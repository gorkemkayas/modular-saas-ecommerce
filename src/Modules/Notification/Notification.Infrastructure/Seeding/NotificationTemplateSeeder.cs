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
                "Your order has been received — {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                We have received your order {{OrderNumber}} successfully. You can find your order summary below.

                We will let you know again as soon as your order starts being prepared.
                """),
            new(
                NotificationTrigger.OrderCancelled,
                "Default Order Cancelled Email",
                "Your order has been cancelled — {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                Your order {{OrderNumber}} has been cancelled.

                If you think this is a mistake, please get in touch with us.
                """),
            new(
                NotificationTrigger.PaymentAuthorized,
                "Default Payment Authorized Email",
                "Your payment is authorized — {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                The payment for your order {{OrderNumber}} has been authorized.
                """),
            new(
                NotificationTrigger.PaymentCaptured,
                "Default Payment Captured Email",
                "Your payment is complete — {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                The payment for your order {{OrderNumber}} has been captured successfully.
                """),
            new(
                NotificationTrigger.PaymentFailed,
                "Default Payment Failed Email",
                "Your payment could not be completed — {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                The payment for your order {{OrderNumber}} could not be completed. Please try again.
                """),
            new(
                NotificationTrigger.PaymentRefunded,
                "Default Payment Refunded Email",
                "Your refund is complete — {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                The refund for your order {{OrderNumber}} has been completed.
                """),
            new(
                NotificationTrigger.ShipmentCreated,
                "Default Shipment Created Email",
                "Your shipment is ready — {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                A shipment has been created for your order {{OrderNumber}}. You can find the details below.
                """),
            new(
                NotificationTrigger.ShipmentShipped,
                "Default Shipment Shipped Email",
                "Your order is on the way — {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                Your order {{OrderNumber}} has been shipped and is on its way.
                """),
            new(
                NotificationTrigger.ShipmentDelivered,
                "Default Shipment Delivered Email",
                "Your order has been delivered — {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                Your order {{OrderNumber}} has been delivered. We hope you enjoy it!
                """),
            new(
                NotificationTrigger.ShipmentDeliveryException,
                "Default Delivery Update Email",
                "Delivery update — {{OrderNumber}}",
                """
                Hi {{RecipientName}},

                There is an update regarding the delivery of your order {{OrderNumber}}. Details are below.
                """)
        ];
    }

    private sealed record NotificationTemplateDefinition(
        NotificationTrigger Trigger,
        string Name,
        string SubjectTemplate,
        string BodyTemplate);
}
