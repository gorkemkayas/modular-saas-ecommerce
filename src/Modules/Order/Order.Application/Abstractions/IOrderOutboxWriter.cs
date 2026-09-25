using Order.Contracts.IntegrationEvents;

namespace Order.Application.Abstractions;

public interface IOrderOutboxWriter
{
    void Add(IOrderIntegrationEvent integrationEvent);
}
