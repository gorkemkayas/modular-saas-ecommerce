using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.ReadServices;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.IntegrationTests.ReadServices
{
    [TestClass]
    public sealed class AttributeDefinitionReadServiceIntegrationTests
    {
        private static CatalogDbContext CreateContext(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new CatalogDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [TestMethod]
        public async Task ListByStoreAsync_WhenActiveOnlyIsTrue_FiltersInactiveDefinitions()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var context = CreateContext(connection);
            var readService = new AttributeDefinitionReadService(context);

            var storeId = Guid.NewGuid();
            var active = AttributeDefinition.Create(storeId, "Color", AttributeCode.Create("color"), AttributeDataType.String);
            var inactive = AttributeDefinition.Create(storeId, "Storage", AttributeCode.Create("storage"), AttributeDataType.String);
            inactive.Deactivate();

            await context.AttributeDefinitions.AddRangeAsync(active, inactive);
            await context.SaveChangesAsync();

            var result = await readService.ListByStoreAsync(storeId, true);

            Assert.HasCount(1, result);
            Assert.AreEqual(active.Id, result.First().Id);
        }
    }
}
