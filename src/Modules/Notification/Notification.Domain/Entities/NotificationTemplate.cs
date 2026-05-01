using Notification.Domain.Common;
using Notification.Domain.Enums;
using Notification.Domain.Exceptions;

namespace Notification.Domain.Entities;

public sealed class NotificationTemplate : IAggregateRoot
{
    private NotificationTemplate()
    {
    }

    private NotificationTemplate(
        Guid id,
        Guid storeId,
        string name,
        NotificationTrigger trigger,
        NotificationChannel channel,
        string locale,
        string subjectTemplate,
        string bodyTemplate)
    {
        if (storeId == Guid.Empty)
            throw new NotificationDomainException("Store id is required.");

        Id = id;
        StoreId = storeId;
        Name = NormalizeRequired(name, "Template name", 200);
        Trigger = trigger;
        Channel = channel;
        Locale = NormalizeRequired(locale, "Locale", 20);
        SubjectTemplate = NormalizeSubject(channel, subjectTemplate);
        BodyTemplate = NormalizeRequired(bodyTemplate, "Body template", 12000);
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Name { get; private set; } = default!;
    public NotificationTrigger Trigger { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Locale { get; private set; } = default!;
    public string SubjectTemplate { get; private set; } = default!;
    public string BodyTemplate { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static NotificationTemplate Create(
        Guid storeId,
        string name,
        NotificationTrigger trigger,
        NotificationChannel channel,
        string locale,
        string subjectTemplate,
        string bodyTemplate)
    {
        return new NotificationTemplate(
            Guid.NewGuid(),
            storeId,
            name,
            trigger,
            channel,
            locale,
            subjectTemplate,
            bodyTemplate);
    }

    public void Update(
        string name,
        string locale,
        string subjectTemplate,
        string bodyTemplate)
    {
        Name = NormalizeRequired(name, "Template name", 200);
        Locale = NormalizeRequired(locale, "Locale", 20);
        SubjectTemplate = NormalizeSubject(Channel, subjectTemplate);
        BodyTemplate = NormalizeRequired(bodyTemplate, "Body template", 12000);
        Touch();
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        Touch();
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string NormalizeSubject(NotificationChannel channel, string value)
    {
        if (channel == NotificationChannel.Email)
            return NormalizeRequired(value, "Subject template", 500);

        return NormalizeOptional(value, 500) ?? string.Empty;
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new NotificationDomainException($"{fieldName} is required.");

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new NotificationDomainException($"{fieldName} cannot exceed {maxLength} characters.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new NotificationDomainException($"Value cannot exceed {maxLength} characters.");

        return normalized;
    }
}
