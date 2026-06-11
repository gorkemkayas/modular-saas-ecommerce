using Notification.Application.Common.Models;
using Notification.Application.Feedbacks.DTOs;
using Notification.Application.Notifications.DTOs;
using Notification.Domain.Enums;

namespace Notification.Application.Abstractions.Queries;

public interface INotificationReadService
{
    Task<NotificationTemplateDto?> GetTemplateByIdAsync(
        Guid storeId,
        Guid templateId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<NotificationTemplateSummaryDto>> SearchTemplatesAsync(
        Guid storeId,
        NotificationTrigger? trigger,
        NotificationChannel? channel,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<NotificationDispatchDto?> GetDispatchByIdAsync(
        Guid storeId,
        Guid dispatchId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<NotificationDispatchSummaryDto>> SearchDispatchesAsync(
        NotificationDispatchSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ContactFeedbackDto>> ListContactFeedbacksAsync(
        CancellationToken cancellationToken = default);
}
