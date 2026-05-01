using MediatR;
using Payment.Domain.Enums;

namespace Payment.Application.Payments.Commands.ProcessPaymentWebhook;

public sealed record ProcessPaymentWebhookCommand(
    PaymentProvider Provider,
    string Payload,
    string? Signature) : IRequest;
