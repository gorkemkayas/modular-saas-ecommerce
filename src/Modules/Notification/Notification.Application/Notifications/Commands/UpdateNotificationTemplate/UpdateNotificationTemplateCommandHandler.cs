using MediatR;
using Notification.Application.Abstractions;
using Notification.Application.Exceptions;
using Notification.Domain.Repositories;

namespace Notification.Application.Notifications.Commands.UpdateNotificationTemplate;

public sealed class UpdateNotificationTemplateCommandHandler : IRequestHandler<UpdateNotificationTemplateCommand>
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNotificationTemplateCommandHandler(
        INotificationTemplateRepository templateRepository,
        IUnitOfWork unitOfWork)
    {
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateNotificationTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(command.StoreId, command.TemplateId, cancellationToken)
            ?? throw new NotificationTemplateNotFoundException(command.TemplateId);

        template.Update(
            command.Name,
            command.Locale,
            command.SubjectTemplate,
            command.BodyTemplate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
