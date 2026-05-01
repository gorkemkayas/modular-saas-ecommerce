using MediatR;
using Notification.Application.Abstractions;
using Notification.Application.Exceptions;
using Notification.Domain.Repositories;

namespace Notification.Application.Notifications.Commands.DeactivateNotificationTemplate;

public sealed class DeactivateNotificationTemplateCommandHandler : IRequestHandler<DeactivateNotificationTemplateCommand>
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateNotificationTemplateCommandHandler(
        INotificationTemplateRepository templateRepository,
        IUnitOfWork unitOfWork)
    {
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateNotificationTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(command.StoreId, command.TemplateId, cancellationToken)
            ?? throw new NotificationTemplateNotFoundException(command.TemplateId);

        template.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
