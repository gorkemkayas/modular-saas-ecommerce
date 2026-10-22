using System;
using BuildingBlocks.Messaging.Abstractions.Events;

namespace Order.Contracts.IntegrationEvents;

public interface IOrderIntegrationEvent : IIntegrationEvent
{
    Guid StoreId { get; }
}
