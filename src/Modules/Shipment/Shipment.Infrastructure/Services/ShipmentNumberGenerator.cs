using Shipment.Application.Integrations;

namespace Shipment.Infrastructure.Services;

public sealed class ShipmentNumberGenerator : IShipmentNumberGenerator
{
    public Task<string> GenerateAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return Task.FromResult($"SHP-{timestamp}-{suffix}");
    }
}
