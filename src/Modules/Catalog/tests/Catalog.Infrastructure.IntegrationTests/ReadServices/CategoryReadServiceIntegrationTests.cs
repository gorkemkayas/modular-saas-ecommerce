using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.ReadServices;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.IntegrationTests.ReadServices
{
    [TestClass]
    public sealed class CategoryReadServiceIntegrationTests
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
        public async Task GetTreeAsync_ReturnsNestedCategoryTree()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var context = CreateContext(connection);
            var readService = new CategoryReadService(context);

            var storeId = Guid.NewGuid();
            var parent = Category.Create(storeId, "Electronics", Slug.Create("electronics"), sortOrder: 0);
            var child = Category.Create(storeId, "Phones", Slug.Create("phones"), parentCategoryId: parent.Id, sortOrder: 0);
            var subChild = Category.Create(storeId, "Smartphones", Slug.Create("smartphones"), parentCategoryId: child.Id, sortOrder: 0);

            await context.Categories.AddRangeAsync(parent, child, subChild);
            await context.SaveChangesAsync();

            var tree = await readService.GetTreeAsync(storeId);

            Assert.HasCount(1, tree);
            Assert.AreEqual(parent.Id, tree.First().Id);
            Assert.HasCount(1, tree.First().Children);
            Assert.AreEqual(child.Id, tree.First().Children.First().Id);
            Assert.HasCount(1, tree.First().Children.First().Children);
            Assert.AreEqual(subChild.Id, tree.First().Children.First().Children.First().Id);
        }
    }
}
