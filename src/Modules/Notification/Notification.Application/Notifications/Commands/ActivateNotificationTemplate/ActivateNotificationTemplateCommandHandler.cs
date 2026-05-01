using MediatR;
using Notification.Application.Abstractions;
using Notification.Application.Exceptions;
using Notification.Domain.Repositories;

namespace Notification.Application.Notifications.Commands.ActivateNotificationTemplate;

public sealed class ActivateNotificationTemplateCommandHandler : IRequestHandler<ActivateNotificationTemplateCommand>
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateNotificationTemplateCommandHandler(
        INotificationTemplateRepository templateRepository,
        IUnitOfWork unitOfWork)
    {
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateNotificationTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(command.StoreId, command.TemplateId, cancellationToken)
            ?? throw new NotificationTemplateNotFoundException(command.TemplateId);

        template.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
