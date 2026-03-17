using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;
using Store.Infrastructure.Persistance;
using Store.Infrastructure.Persistance.Repositories;

namespace Store.Infrastructure.IntegrationTests.Persistance.Repositories
{
    [TestClass]
    public sealed class StoreRepositoryIntegrationTests
    {
        private static StoreDbContext CreateContext(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<StoreDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new StoreDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [TestMethod]
        public async Task AddAsync_AndGetByIdAsync_ReturnsPersistedStore()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var context = CreateContext(connection);
            var repository = new StoreRepository(context);

            var store = Store.Domain.Stores.Store.Create(
                Guid.NewGuid(),
                "Store One",
                Slug.Create("store-one"));

            await repository.AddAsync(store);
            await context.SaveChangesAsync();

            var result = await repository.GetByIdAsync(store.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(store.Id, result.Id);
            Assert.AreEqual(store.TenantId, result.TenantId);
        }

        [TestMethod]
        public async Task GetByTenantIdAsync_WhenStoreExists_ReturnsStore()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var context = CreateContext(connection);
            var repository = new StoreRepository(context);

            var tenantId = Guid.NewGuid();
            var store = Store.Domain.Stores.Store.Create(
                tenantId,
                "Tenant Store",
                Slug.Create("tenant-store"));

            await repository.AddAsync(store);
            await context.SaveChangesAsync();

            var result = await repository.GetByTenantIdAsync(tenantId);

            Assert.IsNotNull(result);
            Assert.AreEqual(tenantId, result.TenantId);
        }

        [TestMethod]
        public async Task ExistsBySlugAsync_WhenStoreDoesNotExist_ReturnsFalse()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var context = CreateContext(connection);
            var repository = new StoreRepository(context);

            var result = await repository.ExistsBySlugAsync(Slug.Create("missing-store"));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task SaveChangesAsync_WhenTenantIdDuplicated_ThrowsDbUpdateException()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var context = CreateContext(connection);
            var repository = new StoreRepository(context);

            var tenantId = Guid.NewGuid();
            var store1 = Store.Domain.Stores.Store.Create(tenantId, "Store A", Slug.Create("store-a"));
            var store2 = Store.Domain.Stores.Store.Create(tenantId, "Store B", Slug.Create("store-b"));

            await repository.AddAsync(store1);
            await context.SaveChangesAsync();

            await repository.AddAsync(store2);

            try
            {
                await context.SaveChangesAsync();
                Assert.Fail("Expected DbUpdateException was not thrown.");
            }
            catch (DbUpdateException)
            {
            }
        }

        [TestMethod]
        public async Task SaveChangesAsync_WhenSlugDuplicated_ThrowsDbUpdateException()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var context = CreateContext(connection);
            var repository = new StoreRepository(context);

            var slug = Slug.Create("same-slug");
            var store1 = Store.Domain.Stores.Store.Create(Guid.NewGuid(), "Store A", slug);
            var store2 = Store.Domain.Stores.Store.Create(Guid.NewGuid(), "Store B", slug);

            await repository.AddAsync(store1);
            await context.SaveChangesAsync();

            await repository.AddAsync(store2);

            try
            {
                await context.SaveChangesAsync();
                Assert.Fail("Expected DbUpdateException was not thrown.");
            }
            catch (DbUpdateException)
            {
            }
        }
    }
}
