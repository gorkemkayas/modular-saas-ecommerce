namespace Shipment.Application.Integrations;

public interface IShipmentNumberGenerator
{
    Task<string> GenerateAsync(Guid storeId, CancellationToken cancellationToken = default);
}
