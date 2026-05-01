using MediatR;
using Notification.Application.Abstractions;
using Notification.Application.Exceptions;
using Notification.Domain.Entities;
using Notification.Domain.Repositories;

namespace Notification.Application.Notifications.Commands.CreateNotificationTemplate;

public sealed class CreateNotificationTemplateCommandHandler : IRequestHandler<CreateNotificationTemplateCommand, Guid>
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNotificationTemplateCommandHandler(
        INotificationTemplateRepository templateRepository,
        IUnitOfWork unitOfWork)
    {
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateNotificationTemplateCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty)
            throw new NotificationTemplateValidationException("StoreId is required.");

        if (await _templateRepository.ExistsByKeyAsync(
                command.StoreId,
                command.Trigger,
                command.Channel,
                command.Locale,
                cancellationToken))
        {
            throw new NotificationTemplateAlreadyExistsException("Notification template already exists for this trigger, channel, and locale.");
        }

        var template = NotificationTemplate.Create(
            command.StoreId,
            command.Name,
            command.Trigger,
            command.Channel,
            command.Locale,
            command.SubjectTemplate,
            command.BodyTemplate);

        await _templateRepository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return template.Id;
    }
}
