using Catalog.Application.Products.DTOs;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Catalog.Infrastructure.ReadServices;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.IntegrationTests.Persistence.Repositories
{
    [TestClass]
    public sealed class ProductRepositoryIntegrationTests
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
        public async Task AddAsync_AndGetByIdAsync_ReturnsProductAggregateWithChildren()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var context = CreateContext(connection);
            var repository = new ProductRepository(context);

            var storeId = Guid.NewGuid();
            var attributeDefinition = AttributeDefinition.Create(
                storeId,
                "Storage",
                AttributeCode.Create("storage"),
                AttributeDataType.String,
                isVariantDefining: true);

            var category = Category.Create(storeId, "Phones", Slug.Create("phones"));

            await context.AttributeDefinitions.AddAsync(attributeDefinition);
            await context.Categories.AddAsync(category);

            var product = Product.CreateVariant(storeId, "iPhone 11", Slug.Create("iphone-11"));
            product.AssignCategories(new[] { category.Id });
            product.AddVariant(
                Sku.Create("IPH11-64"),
                "64 GB",
                0,
                new[] { (attributeDefinition.Id, "64 GB") });
            product.AddMedia(MediaType.Image, "https://cdn.test/iphone11.jpg", "Front", true, 0);

            await repository.AddAsync(product);
            await context.SaveChangesAsync();

            var loaded = await repository.GetByIdAsync(storeId, product.Id);

            Assert.IsNotNull(loaded);
            Assert.AreEqual(product.Id, loaded.Id);
            Assert.HasCount(1, loaded.Categories);
            Assert.HasCount(1, loaded.Variants);
            Assert.HasCount(1, loaded.MediaItems);
            Assert.HasCount(1, loaded.Variants.First().AttributeValues);
        }

        [TestMethod]
        public async Task ExistsBySkuAsync_WhenVariantSkuExistsInStore_ReturnsTrue()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var context = CreateContext(connection);
            var repository = new ProductRepository(context);

            var storeId = Guid.NewGuid();
            var attributeDefinition = AttributeDefinition.Create(
                storeId,
                "Storage",
                AttributeCode.Create("storage"),
                AttributeDataType.String,
                isVariantDefining: true);

            await context.AttributeDefinitions.AddAsync(attributeDefinition);

            var product = Product.CreateVariant(storeId, "iPhone 11", Slug.Create("iphone-11"));
            product.AddVariant(
                Sku.Create("IPH11-128"),
                "128 GB",
                0,
                new[] { (attributeDefinition.Id, "128 GB") });

            await repository.AddAsync(product);
            await context.SaveChangesAsync();

            var exists = await repository.ExistsBySkuAsync(storeId, Sku.Create("IPH11-128"));

            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task SearchAsync_FiltersProductsByCategoryAndPaging()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var context = CreateContext(connection);
            var readService = new ProductReadService(context);

            var storeId = Guid.NewGuid();
            var phoneCategory = Category.Create(storeId, "Phones", Slug.Create("phones"));
            var accessoriesCategory = Category.Create(storeId, "Accessories", Slug.Create("accessories"));

            await context.Categories.AddRangeAsync(phoneCategory, accessoriesCategory);

            var phone = Product.CreateSimple(storeId, "iPhone 11", Slug.Create("iphone-11"), Sku.Create("IPH11"));
            phone.AssignCategories(new[] { phoneCategory.Id });
            phone.Publish();

            var caseProduct = Product.CreateSimple(storeId, "Silicone Case", Slug.Create("silicone-case"), Sku.Create("CASE-01"));
            caseProduct.AssignCategories(new[] { accessoriesCategory.Id });

            await context.Products.AddRangeAsync(phone, caseProduct);
            await context.SaveChangesAsync();

            var result = await readService.SearchAsync(new ProductSearchCriteria(
                storeId,
                "iphone",
                null,
                null,
                null,
                phoneCategory.Id,
                null,
                1,
                10));

            Assert.AreEqual(1, result.TotalCount);
            Assert.HasCount(1, result.Items);
            Assert.AreEqual(phone.Id, result.Items.First().Id);
        }
    }
}
