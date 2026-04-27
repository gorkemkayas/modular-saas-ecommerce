using Order.Application.Integrations;

namespace Order.Infrastructure.Services;

public sealed class OrderNumberGenerator : IOrderNumberGenerator
{
    public Task<string> GenerateAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        var value = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..29].ToUpperInvariant();
        return Task.FromResult(value);
    }
}
