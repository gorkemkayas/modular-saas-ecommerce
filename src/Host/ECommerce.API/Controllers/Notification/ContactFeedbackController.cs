using ECommerce.API.Contracts.Notification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Feedbacks.Commands.SubmitContactFeedback;
using Notification.Application.Feedbacks.DTOs;
using Notification.Application.Feedbacks.Queries.ListContactFeedbacks;

namespace ECommerce.API.Controllers.Notification;

[Route("api/contact-feedback")]
[ApiController]
[AllowAnonymous]
[ApiExplorerSettings(GroupName = "v1")]
public sealed class ContactFeedbackController : ControllerBase
{
    private readonly ISender _sender;

    public ContactFeedbackController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ContactFeedbackDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var feedbacks = await _sender.Send(new ListContactFeedbacksQuery(), cancellationToken);
        return Ok(feedbacks);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SubmitContactFeedbackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitContactFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
            return ValidationProblem(new ValidationProblemDetails(validationErrors));

        var feedbackId = await _sender.Send(
            new SubmitContactFeedbackCommand(
                request.FullName,
                request.Email,
                request.Subject,
                request.Message,
                request.Source),
            cancellationToken);

        return Ok(new SubmitContactFeedbackResponse(feedbackId));
    }

    private static Dictionary<string, string[]> Validate(SubmitContactFeedbackRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.FullName))
            errors["fullName"] = ["Full name is required."];

        if (string.IsNullOrWhiteSpace(request.Email))
            errors["email"] = ["Email is required."];

        if (string.IsNullOrWhiteSpace(request.Subject))
            errors["subject"] = ["Subject is required."];

        if (string.IsNullOrWhiteSpace(request.Message))
            errors["message"] = ["Message is required."];

        return errors;
    }

    public sealed record SubmitContactFeedbackResponse(Guid FeedbackId);
}
