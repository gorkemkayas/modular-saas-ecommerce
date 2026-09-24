using BuildingBlocks.Domain.Events;
using MediatR;

namespace BuildingBlocks.Application.Events;

public sealed record class DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
