using System.Text.Json;
using Notification.Application.Notifications.Services;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Domain.Repositories;
using Notification.Infrastructure.Persistence.Inbox;
using Order.Contracts.IntegrationEvents;

namespace Notification.Infrastructure.Messaging.Inbox;

public sealed class OrderCancelledInboxMessageHandler
    : INotificationInboxMessageHandler
{
    private const string DefaultLocale = "default";
    private const string BusinessEntityType = "Order";

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly INotificationDispatchRepository
        _dispatchRepository;

    private readonly INotificationTemplateRepository
        _templateRepository;

    private readonly ITemplateRenderer _templateRenderer;

    public OrderCancelledInboxMessageHandler(
        INotificationDispatchRepository dispatchRepository,
        INotificationTemplateRepository templateRepository,
        ITemplateRenderer templateRenderer)
    {
        ArgumentNullException.ThrowIfNull(
            dispatchRepository);

        ArgumentNullException.ThrowIfNull(
            templateRepository);

        ArgumentNullException.ThrowIfNull(
            templateRenderer);

        _dispatchRepository = dispatchRepository;
        _templateRepository = templateRepository;
        _templateRenderer = templateRenderer;
    }

    public string EventType =>
        OrderCancelledIntegrationEvent.EventType;

    public async Task HandleAsync(
        ClaimedInboxMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!string.Equals(
                message.EventType,
                EventType,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Handler for '{EventType}' cannot process " +
                $"event type '{message.EventType}'.");
        }

        OrderCancelledIntegrationEvent integrationEvent =
            Deserialize(message.Payload);

        if (integrationEvent.Id != message.Id)
        {
            throw new InvalidDataException(
                "Inbox message id does not match the " +
                "integration event id.");
        }

        NotificationDispatch? existingDispatch =
            await _dispatchRepository.GetByBusinessKeyAsync(
                integrationEvent.StoreId,
                NotificationChannel.Email,
                NotificationTrigger.OrderCancelled,
                BusinessEntityType,
                integrationEvent.OrderId,
                cancellationToken);

        if (existingDispatch is not null)
        {
            return;
        }

        NotificationDispatch dispatch =
            NotificationDispatch.Create(
                integrationEvent.StoreId,
                NotificationChannel.Email,
                NotificationTrigger.OrderCancelled,
                BusinessEntityType,
                integrationEvent.OrderId,
                integrationEvent.CustomerId,
                integrationEvent.RecipientEmail,
                integrationEvent.RecipientName);

        if (string.IsNullOrWhiteSpace(
                integrationEvent.RecipientEmail))
        {
            dispatch.MarkSuppressed(
                "Recipient email address is missing.");

            await _dispatchRepository.AddAsync(
                dispatch,
                cancellationToken);

            return;
        }

        NotificationTemplate? template =
            await ResolveTemplateAsync(
                integrationEvent.StoreId,
                cancellationToken);

        if (template is null)
        {
            dispatch.MarkSuppressed(
                "Active notification template could not be resolved.");

            await _dispatchRepository.AddAsync(
                dispatch,
                cancellationToken);

            return;
        }

        var tokens =
            new Dictionary<string, string?>
            {
                ["RecipientName"] =
                    integrationEvent.RecipientName,

                ["OrderNumber"] =
                    integrationEvent.OrderNumber,

                ["CancellationReason"] =
                    integrationEvent.CancellationReason
            };

        string subject = _templateRenderer.Render(
            template.SubjectTemplate,
            tokens);

        string body = _templateRenderer.Render(
            template.BodyTemplate,
            tokens);

        dispatch.SetRenderedContent(
            subject,
            body);

        await _dispatchRepository.AddAsync(
            dispatch,
            cancellationToken);
    }

    private async Task<NotificationTemplate?>
        ResolveTemplateAsync(
            Guid storeId,
            CancellationToken cancellationToken)
    {
        NotificationTemplate? storeTemplate =
            await _templateRepository.GetActiveAsync(
                storeId,
                NotificationTrigger.OrderCancelled,
                NotificationChannel.Email,
                DefaultLocale,
                cancellationToken);

        if (storeTemplate is not null)
        {
            return storeTemplate;
        }

        return await _templateRepository.GetActiveAsync(
            Guid.Empty,
            NotificationTrigger.OrderCancelled,
            NotificationChannel.Email,
            DefaultLocale,
            cancellationToken);
    }

    private static OrderCancelledIntegrationEvent Deserialize(
        string payload)
    {
        try
        {
            OrderCancelledIntegrationEvent? integrationEvent =
                JsonSerializer.Deserialize<
                    OrderCancelledIntegrationEvent>(
                        payload,
                        JsonOptions);

            return integrationEvent
                ?? throw new InvalidDataException(
                    "Order-cancelled integration event " +
                    "payload cannot be null.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                "Order-cancelled integration event " +
                "payload is invalid.",
                exception);
        }
    }
}