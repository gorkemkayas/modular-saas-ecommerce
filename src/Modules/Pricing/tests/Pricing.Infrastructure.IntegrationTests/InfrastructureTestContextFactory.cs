using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pricing.Infrastructure.Persistence;

namespace Pricing.Infrastructure.IntegrationTests;

internal static class InfrastructureTestContextFactory
{
    public static async Task<(SqliteConnection Connection, PricingDbContext Context)> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PricingDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new PricingDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return (connection, context);
    }
}
