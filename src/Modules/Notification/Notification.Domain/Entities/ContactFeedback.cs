using System.Net.Mail;
using Notification.Domain.Common;
using Notification.Domain.Exceptions;

namespace Notification.Domain.Entities;

public sealed class ContactFeedback : IAggregateRoot
{
    private ContactFeedback()
    {
    }

    private ContactFeedback(
        Guid id,
        string fullName,
        string email,
        string subject,
        string message,
        string? source)
    {
        Id = id;
        FullName = NormalizeRequired(fullName, "Full name", 200);
        Email = NormalizeEmail(email);
        Subject = NormalizeRequired(subject, "Subject", 200);
        Message = NormalizeRequired(message, "Message", 4000);
        Source = NormalizeOptional(source, "Source", 100);
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Subject { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public string? Source { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static ContactFeedback Create(
        string fullName,
        string email,
        string subject,
        string message,
        string? source)
    {
        return new ContactFeedback(
            Guid.NewGuid(),
            fullName,
            email,
            subject,
            message,
            source);
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

    private static string? NormalizeOptional(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new NotificationDomainException($"{fieldName} cannot exceed {maxLength} characters.");

        return normalized;
    }

    private static string NormalizeEmail(string value)
    {
        var normalized = NormalizeRequired(value, "Email", 320);

        if (!MailAddress.TryCreate(normalized, out _))
            throw new NotificationDomainException("Email address is not valid.");

        return normalized;
    }
}
