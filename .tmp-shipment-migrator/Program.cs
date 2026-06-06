using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Order.Infrastructure.Persistence;
using Shipment.Infrastructure.Persistence;

var repositoryRoot = Directory.GetCurrentDirectory();
var hostProjectPath = Path.Combine(repositoryRoot, "src", "Host", "ECommerce.API");
var userSecretsPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "Microsoft",
    "UserSecrets",
    "716d3c42-2c8e-4814-914e-b9e97da19bc3",
    "secrets.json");

var configuration = new ConfigurationBuilder()
    .SetBasePath(hostProjectPath)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddJsonFile(userSecretsPath, optional: true)
    .AddEnvironmentVariables()
    .Build();

await ApplyMigrationAsync(
    "Order",
    configuration["Modules:Order:Database:ConnectionString"],
    connectionString =>
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new OrderDbContext(options);
    });

await ApplyMigrationAsync(
    "Shipment",
    configuration["Modules:Shipment:Database:ConnectionString"],
    connectionString =>
    {
        var options = new DbContextOptionsBuilder<ShipmentDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ShipmentDbContext(options);
    });

static async Task ApplyMigrationAsync<TContext>(
    string moduleName,
    string? connectionString,
    Func<string, TContext> createContext)
    where TContext : DbContext
{
    if (string.IsNullOrWhiteSpace(connectionString) ||
        string.Equals(connectionString, "SET_VIA_USER_SECRETS", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"{moduleName} connection string is not configured.");
    }

    await using var context = createContext(connectionString);

    var knownMigrations = context.Database.GetMigrations().ToArray();
    var appliedBefore = context.Database.GetAppliedMigrations().ToArray();
    var pendingBefore = context.Database.GetPendingMigrations().ToArray();

    Console.WriteLine($"{moduleName}: known={knownMigrations.Length}, appliedBefore={appliedBefore.Length}, pendingBefore={pendingBefore.Length}");
    foreach (var migration in pendingBefore)
    {
        Console.WriteLine($"{moduleName}: pending {migration}");
    }

    await context.Database.MigrateAsync();

    var appliedAfter = context.Database.GetAppliedMigrations().ToArray();
    Console.WriteLine($"{moduleName}: appliedAfter={appliedAfter.Length}");
}
