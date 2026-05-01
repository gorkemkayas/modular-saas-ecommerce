using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Domain.Repositories;

namespace Notification.Application.Notifications.Services;

public sealed class NotificationSender : INotificationSender
{
    private const string DefaultLocale = "default";

    private readonly INotificationTemplateRepository _templateRepository;
    private readonly INotificationDispatchRepository _dispatchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IEmailGateway _emailGateway;
    private readonly ILogger<NotificationSender> _logger;

    public NotificationSender(
        INotificationTemplateRepository templateRepository,
        INotificationDispatchRepository dispatchRepository,
        IUnitOfWork unitOfWork,
        ITemplateRenderer templateRenderer,
        IEmailGateway emailGateway,
        ILogger<NotificationSender> logger)
    {
        _templateRepository = templateRepository;
        _dispatchRepository = dispatchRepository;
        _unitOfWork = unitOfWork;
        _templateRenderer = templateRenderer;
        _emailGateway = emailGateway;
        _logger = logger;
    }

    public async Task<Guid> SendAsync(
        TransactionalNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var existingDispatch = await _dispatchRepository.GetByBusinessKeyAsync(
            request.StoreId,
            request.Channel,
            request.Trigger,
            request.BusinessEntityType,
            request.BusinessEntityId,
            cancellationToken);

        if (existingDispatch is not null)
        {
            _logger.LogInformation(
                "Notification dispatch already exists | DispatchId: {DispatchId} | StoreId: {StoreId} | Trigger: {Trigger} | BusinessEntityType: {BusinessEntityType} | BusinessEntityId: {BusinessEntityId}",
                existingDispatch.Id,
                request.StoreId,
                request.Trigger,
                request.BusinessEntityType,
                request.BusinessEntityId);

            return existingDispatch.Id;
        }

        var dispatch = NotificationDispatch.Create(
            request.StoreId,
            request.Channel,
            request.Trigger,
            request.BusinessEntityType,
            request.BusinessEntityId,
            request.CustomerId,
            request.RecipientAddress,
            request.RecipientName);

        await _dispatchRepository.AddAsync(dispatch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.Channel != NotificationChannel.Email)
        {
            dispatch.MarkSuppressed("Only email notifications are supported in the current MVP implementation.");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return dispatch.Id;
        }

        if (string.IsNullOrWhiteSpace(request.RecipientAddress))
        {
            dispatch.MarkSuppressed("Recipient email address is missing.");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return dispatch.Id;
        }

        var template = await ResolveTemplateAsync(request, cancellationToken);

        if (template is null)
        {
            dispatch.MarkSuppressed("Active notification template could not be resolved.");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return dispatch.Id;
        }

        var subject = _templateRenderer.Render(template.SubjectTemplate, request.Tokens);
        var body = _templateRenderer.Render(template.BodyTemplate, request.Tokens);

        dispatch.SetRenderedContent(subject, body);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var sendResult = await _emailGateway.SendAsync(
                new EmailSendRequest(
                    request.StoreId,
                    request.RecipientAddress,
                    request.RecipientName,
                    subject,
                    body),
                cancellationToken);

            if (sendResult.IsSuccess)
            {
                dispatch.MarkSent(
                    _emailGateway.ProviderName,
                    sendResult.ProviderRequestReference,
                    sendResult.ProviderMessageId);
            }
            else
            {
                dispatch.MarkFailed(
                    _emailGateway.ProviderName,
                    sendResult.ProviderRequestReference,
                    sendResult.FailureCode,
                    sendResult.FailureMessage);
            }
        }
        catch (Exception exception)
        {
            dispatch.MarkFailed(
                _emailGateway.ProviderName,
                null,
                "notification_send_error",
                exception.Message);

            _logger.LogWarning(
                exception,
                "Notification send failed with exception | DispatchId: {DispatchId} | StoreId: {StoreId} | Trigger: {Trigger}",
                dispatch.Id,
                dispatch.StoreId,
                dispatch.Trigger);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return dispatch.Id;
    }

    private async Task<NotificationTemplate?> ResolveTemplateAsync(
        TransactionalNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var locale = string.IsNullOrWhiteSpace(request.Locale)
            ? DefaultLocale
            : request.Locale.Trim();

        var candidateStoreIds = new[] { request.StoreId, Guid.Empty }.Distinct().ToArray();
        var candidateLocales = string.Equals(locale, DefaultLocale, StringComparison.OrdinalIgnoreCase)
            ? new[] { DefaultLocale }
            : new[] { locale, DefaultLocale };

        foreach (var storeId in candidateStoreIds)
        {
            foreach (var candidateLocale in candidateLocales)
            {
                var template = await _templateRepository.GetActiveAsync(
                    storeId,
                    request.Trigger,
                    request.Channel,
                    candidateLocale,
                    cancellationToken);

                if (template is not null)
                    return template;
            }
        }

        return null;
    }
}
