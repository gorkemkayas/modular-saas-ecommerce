using Testcontainers.PostgreSql;

namespace Order.Infrastructure.IntegrationTests;

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
                .WithDatabase("order_integration_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

        await _container.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task CleanupAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}