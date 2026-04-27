namespace Order.Application.Integrations;

public interface IOrderNumberGenerator
{
    Task<string> GenerateAsync(Guid storeId, CancellationToken cancellationToken = default);
}
