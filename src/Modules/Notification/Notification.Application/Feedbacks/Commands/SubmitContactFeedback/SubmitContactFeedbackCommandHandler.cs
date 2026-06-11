using MediatR;
using Notification.Application.Abstractions;
using Notification.Domain.Entities;
using Notification.Domain.Repositories;

namespace Notification.Application.Feedbacks.Commands.SubmitContactFeedback;

public sealed class SubmitContactFeedbackCommandHandler : IRequestHandler<SubmitContactFeedbackCommand, Guid>
{
    private readonly IContactFeedbackRepository _feedbackRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitContactFeedbackCommandHandler(
        IContactFeedbackRepository feedbackRepository,
        IUnitOfWork unitOfWork)
    {
        _feedbackRepository = feedbackRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(SubmitContactFeedbackCommand command, CancellationToken cancellationToken)
    {
        var feedback = ContactFeedback.Create(
            command.FullName,
            command.Email,
            command.Subject,
            command.Message,
            command.Source);

        await _feedbackRepository.AddAsync(feedback, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return feedback.Id;
    }
}
