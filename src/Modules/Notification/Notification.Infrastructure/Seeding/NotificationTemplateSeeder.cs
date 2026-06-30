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
                "Varsayılan Sipariş Alındı E-postası",
                "Siparişin alındı — {{OrderNumber}}",
                """
                Merhaba {{RecipientName}},

                {{OrderNumber}} numaralı siparişini başarıyla aldık. Sipariş özetini aşağıda bulabilirsin.

                Siparişin hazırlanmaya başlandığında seni tekrar bilgilendireceğiz.
                """),
            new(
                NotificationTrigger.OrderCancelled,
                "Varsayılan Sipariş İptal E-postası",
                "Siparişin iptal edildi — {{OrderNumber}}",
                """
                Merhaba {{RecipientName}},

                {{OrderNumber}} numaralı siparişin iptal edildi.

                Bir hata olduğunu düşünüyorsan bizimle iletişime geçebilirsin.
                """),
            new(
                NotificationTrigger.PaymentAuthorized,
                "Varsayılan Ödeme Onayı E-postası",
                "Ödemen onaylandı — {{OrderNumber}}",
                """
                Merhaba {{RecipientName}},

                {{OrderNumber}} numaralı siparişin için ödemen onaylandı.
                """),
            new(
                NotificationTrigger.PaymentCaptured,
                "Varsayılan Ödeme Tahsilat E-postası",
                "Ödemen alındı — {{OrderNumber}}",
                """
                Merhaba {{RecipientName}},

                {{OrderNumber}} numaralı siparişin için ödemen başarıyla tahsil edildi.
                """),
            new(
                NotificationTrigger.PaymentFailed,
                "Varsayılan Ödeme Başarısız E-postası",
                "Ödemen tamamlanamadı — {{OrderNumber}}",
                """
                Merhaba {{RecipientName}},

                {{OrderNumber}} numaralı siparişin için ödemen tamamlanamadı. Lütfen tekrar dener misin?
                """),
            new(
                NotificationTrigger.PaymentRefunded,
                "Varsayılan İade E-postası",
                "İaden tamamlandı — {{OrderNumber}}",
                """
                Merhaba {{RecipientName}},

                {{OrderNumber}} numaralı siparişin için iaden tamamlandı.
                """),
            new(
                NotificationTrigger.ShipmentCreated,
                "Varsayılan Gönderi Oluşturuldu E-postası",
                "Gönderin hazırlandı — {{OrderNumber}}",
                """
                Merhaba {{RecipientName}},

                {{OrderNumber}} numaralı siparişin için bir gönderi oluşturuldu. Detayları aşağıda bulabilirsin.
                """),
            new(
                NotificationTrigger.ShipmentShipped,
                "Varsayılan Kargoya Verildi E-postası",
                "Siparişin yola çıktı — {{OrderNumber}}",
                """
                Merhaba {{RecipientName}},

                {{OrderNumber}} numaralı siparişin kargoya verildi ve yola çıktı.
                """),
            new(
                NotificationTrigger.ShipmentDelivered,
                "Varsayılan Teslim Edildi E-postası",
                "Siparişin teslim edildi — {{OrderNumber}}",
                """
                Merhaba {{RecipientName}},

                {{OrderNumber}} numaralı siparişin teslim edildi. Keyifle kullanmanı dileriz!
                """),
            new(
                NotificationTrigger.ShipmentDeliveryException,
                "Varsayılan Teslimat Güncellemesi E-postası",
                "Teslimat güncellemesi — {{OrderNumber}}",
                """
                Merhaba {{RecipientName}},

                {{OrderNumber}} numaralı siparişinin teslimatıyla ilgili bir güncelleme var. Detaylar aşağıda.
                """)
        ];
    }

    private sealed record NotificationTemplateDefinition(
        NotificationTrigger Trigger,
        string Name,
        string SubjectTemplate,
        string BodyTemplate);
}
