using Microsoft.EntityFrameworkCore;
using Notification.Application.Abstractions.Queries;
using Notification.Application.Common.Models;
using Notification.Application.Feedbacks.DTOs;
using Notification.Application.Notifications.DTOs;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.ReadServices;

public sealed class NotificationReadService : INotificationReadService
{
    private readonly NotificationDbContext _context;

    public NotificationReadService(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationTemplateDto?> GetTemplateByIdAsync(
        Guid storeId,
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await _context.NotificationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == templateId, cancellationToken);

        return template is null
            ? null
            : new NotificationTemplateDto(
                template.Id,
                template.StoreId,
                template.Trigger,
                template.Channel,
                template.Locale,
                template.Name,
                template.SubjectTemplate,
                template.BodyTemplate,
                template.IsActive,
                template.CreatedAtUtc,
                template.UpdatedAtUtc);
    }

    public async Task<IReadOnlyCollection<NotificationTemplateSummaryDto>> SearchTemplatesAsync(
        Guid storeId,
        Domain.Enums.NotificationTrigger? trigger,
        Domain.Enums.NotificationChannel? channel,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.NotificationTemplates
            .AsNoTracking()
            .Where(x => x.StoreId == storeId);

        if (trigger.HasValue)
            query = query.Where(x => x.Trigger == trigger.Value);

        if (channel.HasValue)
            query = query.Where(x => x.Channel == channel.Value);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        return await query
            .OrderBy(x => x.Trigger)
            .ThenBy(x => x.Locale)
            .Select(x => new NotificationTemplateSummaryDto(
                x.Id,
                x.Trigger,
                x.Channel,
                x.Locale,
                x.Name,
                x.IsActive,
                x.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<NotificationDispatchDto?> GetDispatchByIdAsync(
        Guid storeId,
        Guid dispatchId,
        CancellationToken cancellationToken = default)
    {
        var dispatch = await _context.NotificationDispatches
            .AsNoTracking()
            .Include(x => x.Attempts)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == dispatchId, cancellationToken);

        return dispatch is null ? null : MapDispatch(dispatch);
    }

    public async Task<PagedResult<NotificationDispatchSummaryDto>> SearchDispatchesAsync(
        NotificationDispatchSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = _context.NotificationDispatches
            .AsNoTracking()
            .Where(x => x.StoreId == criteria.StoreId);

        if (criteria.Trigger.HasValue)
            query = query.Where(x => x.Trigger == criteria.Trigger.Value);

        if (criteria.Channel.HasValue)
            query = query.Where(x => x.Channel == criteria.Channel.Value);

        if (criteria.Status.HasValue)
            query = query.Where(x => x.Status == criteria.Status.Value);

        if (!string.IsNullOrWhiteSpace(criteria.BusinessEntityType))
        {
            var businessEntityType = criteria.BusinessEntityType.Trim();
            query = query.Where(x => x.BusinessEntityType == businessEntityType);
        }

        if (criteria.BusinessEntityId.HasValue)
            query = query.Where(x => x.BusinessEntityId == criteria.BusinessEntityId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(x => new NotificationDispatchSummaryDto(
                x.Id,
                x.Channel,
                x.Trigger,
                x.Status,
                x.RecipientAddress,
                x.BusinessEntityType,
                x.BusinessEntityId,
                x.ProviderName,
                x.LastProviderEventType,
                x.CreatedAtUtc,
                x.SentAtUtc))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<NotificationDispatchSummaryDto>(
            items,
            criteria.PageNumber,
            criteria.PageSize,
            totalCount);
    }

    public async Task<IReadOnlyCollection<ContactFeedbackDto>> ListContactFeedbacksAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ContactFeedbacks
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ContactFeedbackDto(
                x.Id,
                x.FullName,
                x.Subject,
                x.Message,
                x.Source,
                x.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);
    }

    private static NotificationDispatchDto MapDispatch(Domain.Entities.NotificationDispatch dispatch)
    {
        return new NotificationDispatchDto(
            dispatch.Id,
            dispatch.StoreId,
            dispatch.Channel,
            dispatch.Trigger,
            dispatch.Status,
            dispatch.RecipientAddress,
            dispatch.RecipientName,
            dispatch.Subject,
            dispatch.Body,
            dispatch.BusinessEntityType,
            dispatch.BusinessEntityId,
            dispatch.CustomerId,
            dispatch.ProviderName,
            dispatch.ProviderMessageId,
            dispatch.FailureCode,
            dispatch.FailureMessage,
            dispatch.SuppressionReason,
            dispatch.LastProviderEventType,
            dispatch.CreatedAtUtc,
            dispatch.UpdatedAtUtc,
            dispatch.SentAtUtc,
            dispatch.LastAttemptAtUtc,
            dispatch.LastProviderEventAtUtc,
            dispatch.DeliveredAtUtc,
            dispatch.OpenedAtUtc,
            dispatch.ClickedAtUtc,
            dispatch.BouncedAtUtc,
            dispatch.ComplainedAtUtc,
            dispatch.Attempts
                .OrderBy(x => x.AttemptNumber)
                .Select(x => new NotificationAttemptDto(
                    x.Id,
                    x.AttemptNumber,
                    x.Status,
                    x.ProviderName,
                    x.ProviderRequestReference,
                    x.ProviderMessageId,
                    x.FailureCode,
                    x.FailureMessage,
                    x.AttemptedAtUtc))
                .ToArray());
    }
}
