using Testcontainers.PostgreSql;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class PostgreSqlTestContainer
{
    private static PostgreSqlContainer? _container;

    internal static string ConnectionString =>
        _container?.GetConnectionString()
        ?? throw new InvalidOperationException(
            "The PostgreSQL test container has not been initialized.");

    [AssemblyInitialize]
    public static async Task InitializeAsync(TestContext _)
    {
        _container =
            new PostgreSqlBuilder("postgres:17-alpine")
                .WithDatabase("notification_integration_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

        await Task.WhenAll(
            _container.StartAsync(),
            RabbitMqTestContainer.InitializeAsync());
    }

    [AssemblyCleanup]
    public static async Task CleanupAsync()
    {
        await RabbitMqTestContainer.CleanupAsync();

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
