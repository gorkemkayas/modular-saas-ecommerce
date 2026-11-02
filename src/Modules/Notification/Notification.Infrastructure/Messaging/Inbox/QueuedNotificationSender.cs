using System.Text.Json;
using Notification.Application.Notifications.Services;
using Notification.Domain.Entities;
using Notification.Domain.Repositories;
using Notification.Infrastructure.Persistence.Dispatch;

namespace Notification.Infrastructure.Messaging.Inbox;

// Participates in the Inbox processor's transaction; never saves or sends itself.
public sealed class QueuedNotificationSender : INotificationSender
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly INotificationDispatchRepository _dispatches;
    private readonly INotificationTemplateRepository _templates;
    private readonly ITemplateRenderer _renderer;
    private readonly INotificationDispatchStore _delivery;
    private readonly TimeProvider _clock;

    public QueuedNotificationSender(INotificationDispatchRepository dispatches,
        INotificationTemplateRepository templates, ITemplateRenderer renderer,
        INotificationDispatchStore delivery, TimeProvider clock)
    {
        _dispatches = dispatches;
        _templates = templates;
        _renderer = renderer;
        _delivery = delivery;
        _clock = clock;
    }

    public async Task<Guid> SendAsync(TransactionalNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _dispatches.GetByBusinessKeyAsync(request.StoreId, request.Channel,
            request.Trigger, request.BusinessEntityType, request.BusinessEntityId, cancellationToken);
        if (existing is not null) return existing.Id;

        var dispatch = NotificationDispatch.Create(request.StoreId, request.Channel, request.Trigger,
            request.BusinessEntityType, request.BusinessEntityId, request.CustomerId,
            request.RecipientAddress, request.RecipientName);
        await _dispatches.AddAsync(dispatch, cancellationToken);
        if (string.IsNullOrWhiteSpace(request.RecipientAddress))
        {
            dispatch.MarkSuppressed("Recipient email address is missing.");
            return dispatch.Id;
        }

        var template = await _templates.GetActiveAsync(request.StoreId, request.Trigger, request.Channel, request.Locale, cancellationToken)
            ?? await _templates.GetActiveAsync(Guid.Empty, request.Trigger, request.Channel, request.Locale, cancellationToken);
        if (template is null)
        {
            dispatch.MarkSuppressed("Active notification template could not be resolved.");
            return dispatch.Id;
        }

        dispatch.SetRenderedContent(_renderer.Render(template.SubjectTemplate, request.Tokens),
            _renderer.Render(template.BodyTemplate, request.Tokens));
        await _delivery.EnqueueAsync(dispatch.Id, _clock.GetUtcNow().UtcDateTime, cancellationToken,
            request.Content is null ? null : JsonSerializer.Serialize(request.Content, JsonOptions));
        return dispatch.Id;
    }
}
