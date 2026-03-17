using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application.DTOs;
using Store.Application.Stores.Queries.GetStoreBySlug;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;
using StoreEntity = Store.Domain.Stores.Store;

namespace Store.Application.Stores.Queries.GetStoreBySlug.UnitTests
{
    [TestClass]
    public sealed class GetStoreBySlugQueryHandlerTests
    {
        /// <summary>
        /// Tests that Handle returns a valid StoreDto when the store exists in the repository.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidSlugAndStoreExists_ReturnsStoreDto()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var handler = new GetStoreBySlugQueryHandler(mockRepository.Object);

            var tenantId = Guid.NewGuid();
            var storeId = Guid.NewGuid();
            var slugValue = "valid-store-slug";
            var slug = Slug.Create(slugValue);
            var store = StoreEntity.Create(tenantId, "Test Store", slug, "Test Description", "https://logo.url");

            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var query = new GetStoreBySlugQuery(slugValue);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(store.Id, result.Id);
            Assert.AreEqual(store.TenantId, result.TenantId);
            Assert.AreEqual(store.Name, result.Name);
            Assert.AreEqual(store.Slug.Value, result.Slug);
            Assert.AreEqual(store.Description, result.Description);
            Assert.AreEqual(store.LogoUrl, result.LogoUrl);
            Assert.AreEqual(store.Status, result.Status);
            Assert.AreEqual(store.IsPublished, result.IsPublished);
        }

        /// <summary>
        /// Tests that Handle returns null when the store does not exist in the repository.
        /// </summary>
        [TestMethod]
        public async Task Handle_StoreNotFound_ReturnsNull()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var handler = new GetStoreBySlugQueryHandler(mockRepository.Object);

            var slugValue = "non-existent-slug";

            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((StoreEntity?)null);

            var query = new GetStoreBySlugQuery(slugValue);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Handle correctly maps nullable Description and LogoUrl properties when they are null.
        /// </summary>
        [TestMethod]
        public async Task Handle_StoreWithNullableProperties_MapsCorrectly()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var handler = new GetStoreBySlugQueryHandler(mockRepository.Object);

            var tenantId = Guid.NewGuid();
            var slugValue = "store-without-details";
            var slug = Slug.Create(slugValue);
            var store = StoreEntity.Create(tenantId, "Minimal Store", slug, null, null);

            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var query = new GetStoreBySlugQuery(slugValue);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Description);
            Assert.IsNull(result.LogoUrl);
        }

        /// <summary>
        /// Tests that Handle passes the cancellation token to the repository method.
        /// </summary>
        [TestMethod]
        public async Task Handle_PassesCancellationTokenToRepository()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var handler = new GetStoreBySlugQueryHandler(mockRepository.Object);

            var tenantId = Guid.NewGuid();
            var slugValue = "test-slug";
            var slug = Slug.Create(slugValue);
            var store = StoreEntity.Create(tenantId, "Test Store", slug);
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), cancellationToken))
                .ReturnsAsync(store);

            var query = new GetStoreBySlugQuery(slugValue);

            // Act
            await handler.Handle(query, cancellationToken);

            // Assert
            mockRepository.Verify(r => r.GetBySlugAsync(It.IsAny<Slug>(), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that Handle works correctly with slug containing numbers.
        /// </summary>
        [TestMethod]
        public async Task Handle_SlugWithNumbers_ReturnsStoreDto()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var handler = new GetStoreBySlugQueryHandler(mockRepository.Object);

            var tenantId = Guid.NewGuid();
            var slugValue = "store-123-test";
            var slug = Slug.Create(slugValue);
            var store = StoreEntity.Create(tenantId, "Store 123", slug);

            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var query = new GetStoreBySlugQuery(slugValue);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(store.Id, result.Id);
        }

        /// <summary>
        /// Tests that Handle normalizes slug to lowercase before querying the repository.
        /// </summary>
        [TestMethod]
        public async Task Handle_SlugWithMixedCase_NormalizesToLowercase()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var handler = new GetStoreBySlugQueryHandler(mockRepository.Object);

            var tenantId = Guid.NewGuid();
            var slugValue = "test-store";
            var slug = Slug.Create(slugValue);
            var store = StoreEntity.Create(tenantId, "Test Store", slug);

            Slug? capturedSlug = null;
            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .Callback<Slug, CancellationToken>((s, ct) => capturedSlug = s)
                .ReturnsAsync(store);

            var query = new GetStoreBySlugQuery("Test-Store");
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNotNull(capturedSlug);
            Assert.AreEqual("test-store", capturedSlug.Value);
        }

        /// <summary>
        /// Tests that Handle works correctly with very long valid slug.
        /// </summary>
        [TestMethod]
        public async Task Handle_VeryLongValidSlug_ReturnsStoreDto()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var handler = new GetStoreBySlugQueryHandler(mockRepository.Object);

            var tenantId = Guid.NewGuid();
            var slugValue = "this-is-a-very-long-slug-with-many-words-separated-by-hyphens-to-test-edge-cases";
            var slug = Slug.Create(slugValue);
            var store = StoreEntity.Create(tenantId, "Long Name Store", slug);

            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var query = new GetStoreBySlugQuery(slugValue);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(store.Id, result.Id);
        }

        /// <summary>
        /// Tests that Handle correctly trims whitespace from slug input.
        /// </summary>
        [TestMethod]
        public async Task Handle_SlugWithLeadingAndTrailingWhitespace_TrimsAndSucceeds()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var handler = new GetStoreBySlugQueryHandler(mockRepository.Object);

            var tenantId = Guid.NewGuid();
            var slugValue = "test-slug";
            var slug = Slug.Create(slugValue);
            var store = StoreEntity.Create(tenantId, "Test Store", slug);

            Slug? capturedSlug = null;
            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .Callback<Slug, CancellationToken>((s, ct) => capturedSlug = s)
                .ReturnsAsync(store);

            var query = new GetStoreBySlugQuery("  test-slug  ");
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNotNull(capturedSlug);
            Assert.AreEqual("test-slug", capturedSlug.Value);
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests that Handle works with default cancellation token.
        /// </summary>
        [TestMethod]
        public async Task Handle_DefaultCancellationToken_ReturnsStoreDto()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var handler = new GetStoreBySlugQueryHandler(mockRepository.Object);

            var tenantId = Guid.NewGuid();
            var slugValue = "test-slug";
            var slug = Slug.Create(slugValue);
            var store = StoreEntity.Create(tenantId, "Test Store", slug);

            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var query = new GetStoreBySlugQuery(slugValue);

            // Act
            var result = await handler.Handle(query);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(store.Id, result.Id);
        }

        /// <summary>
        /// Tests that Handle works correctly with single-character slug.
        /// </summary>
        [TestMethod]
        public async Task Handle_SingleCharacterSlug_ReturnsStoreDto()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var handler = new GetStoreBySlugQueryHandler(mockRepository.Object);

            var tenantId = Guid.NewGuid();
            var slugValue = "a";
            var slug = Slug.Create(slugValue);
            var store = StoreEntity.Create(tenantId, "Store A", slug);

            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var query = new GetStoreBySlugQuery(slugValue);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(store.Id, result.Id);
        }

        /// <summary>
        /// Tests that Handle works correctly with numeric-only slug.
        /// </summary>
        [TestMethod]
        public async Task Handle_NumericOnlySlug_ReturnsStoreDto()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var handler = new GetStoreBySlugQueryHandler(mockRepository.Object);

            var tenantId = Guid.NewGuid();
            var slugValue = "12345";
            var slug = Slug.Create(slugValue);
            var store = StoreEntity.Create(tenantId, "Store 12345", slug);

            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var query = new GetStoreBySlugQuery(slugValue);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(store.Id, result.Id);
        }

        /// <summary>
        /// Tests that the constructor successfully initializes the handler with a valid store repository.
        /// Input: Valid mocked IStoreRepository instance.
        /// Expected: Handler is created without throwing any exceptions.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidStoreRepository_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockStoreRepository = new Mock<IStoreRepository>();

            // Act
            var handler = new GetStoreBySlugQueryHandler(mockStoreRepository.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor accepts a null store repository parameter.
        /// Input: Null IStoreRepository.
        /// Expected: Constructor does not throw an exception as there is no null validation in the implementation.
        /// </summary>
        [TestMethod]
        public void Constructor_NullStoreRepository_DoesNotThrowException()
        {
            // Arrange
            IStoreRepository? nullRepository = null;

            // Act
            var handler = new GetStoreBySlugQueryHandler(nullRepository!);

            // Assert
            Assert.IsNotNull(handler);
        }
    }
}