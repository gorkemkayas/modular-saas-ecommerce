using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application;
using Store.Application.DTOs;
using Store.Application.Stores.Queries;
using Store.Application.Stores.Queries.GetPublishedStorefrontBySlug;
using Store.Domain;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Queries.GetPublishedStorefrontBySlug.UnitTests
{
    /// <summary>
    /// Unit tests for GetPublishedStoreFrontBySlugQueryHandler.
    /// </summary>
    [TestClass]
    public sealed class GetPublishedStoreFrontBySlugQueryHandlerTests
    {
        /// <summary>
        /// Tests that Handle returns null when the store is not found by the repository.
        /// </summary>
        [TestMethod]
        public async Task Handle_StoreNotFound_ReturnsNull()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var slug = Slug.Create("valid-slug");
            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Stores.Store?)null);

            var handler = new GetPublishedStoreFrontBySlugQueryHandler(mockRepository.Object);
            var query = new GetPublishedStoreFrontBySlugQuery("valid-slug");
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNull(result);
            mockRepository.Verify(
                r => r.GetBySlugAsync(It.IsAny<Slug>(), cancellationToken),
                Times.Once);
        }

        /// <summary>
        /// Tests that Handle returns null when the store is not published, regardless of status.
        /// </summary>
        /// <param name="status">The store status to test.</param>
        [TestMethod]
        [DataRow(StoreStatus.Active)]
        [DataRow(StoreStatus.Suspended)]
        [DataRow(StoreStatus.Archived)]
        public async Task Handle_StoreNotPublished_ReturnsNull(StoreStatus status)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slugValue = "test-store";
            var slug = Slug.Create(slugValue);
            var store = Domain.Stores.Store.Create(tenantId, "Test Store", slug, "Description", "logo.png");

            // Ensure store is not published
            if (status == StoreStatus.Suspended)
            {
                store.Suspend();
            }
            else if (status == StoreStatus.Archived)
            {
                store.Archive();
            }
            // For Active status, IsPublished is false by default after Create

            var mockRepository = new Mock<IStoreRepository>();
            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var handler = new GetPublishedStoreFrontBySlugQueryHandler(mockRepository.Object);
            var query = new GetPublishedStoreFrontBySlugQuery(slugValue);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Handle returns null when the store is published but status is not Active.
        /// </summary>
        /// <param name="status">The non-Active store status to test.</param>
        [TestMethod]
        [DataRow(StoreStatus.Suspended)]
        [DataRow(StoreStatus.Archived)]
        public async Task Handle_PublishedStoreButNotActive_ReturnsNull(StoreStatus status)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slugValue = "test-store";
            var slug = Slug.Create(slugValue);
            var store = Domain.Stores.Store.Create(tenantId, "Test Store", slug, "Description", "logo.png");

            // Publish first (requires Active status)
            store.Activate();
            store.Publish();

            // Then change status
            if (status == StoreStatus.Suspended)
            {
                store.Suspend();
            }
            else if (status == StoreStatus.Archived)
            {
                store.Archive();
            }

            var mockRepository = new Mock<IStoreRepository>();
            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var handler = new GetPublishedStoreFrontBySlugQueryHandler(mockRepository.Object);
            var query = new GetPublishedStoreFrontBySlugQuery(slugValue);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Handle returns a StorefrontDto when the store is published and active.
        /// </summary>
        [TestMethod]
        public async Task Handle_PublishedAndActiveStore_ReturnsStorefrontDto()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slugValue = "test-store";
            var storeName = "Test Store";
            var description = "Test Description";
            var logoUrl = "https://example.com/logo.png";
            var slug = Slug.Create(slugValue);
            var store = Domain.Stores.Store.Create(tenantId, storeName, slug, description, logoUrl);
            store.Activate();
            store.Publish();

            var mockRepository = new Mock<IStoreRepository>();
            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var handler = new GetPublishedStoreFrontBySlugQueryHandler(mockRepository.Object);
            var query = new GetPublishedStoreFrontBySlugQuery(slugValue);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(tenantId, result.TenantId);
            Assert.AreEqual(storeName, result.Name);
            Assert.AreEqual(slugValue, result.Slug);
            Assert.AreEqual(description, result.Description);
            Assert.AreEqual(logoUrl, result.LogoUrl);
        }

        /// <summary>
        /// Tests that Handle returns a StorefrontDto with null optional fields when store has no description or logo.
        /// </summary>
        [TestMethod]
        public async Task Handle_PublishedAndActiveStoreWithNullOptionalFields_ReturnsStorefrontDtoWithNulls()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slugValue = "test-store";
            var storeName = "Test Store";
            var slug = Slug.Create(slugValue);
            var store = Domain.Stores.Store.Create(tenantId, storeName, slug, null, null);
            store.Activate();
            store.Publish();

            var mockRepository = new Mock<IStoreRepository>();
            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var handler = new GetPublishedStoreFrontBySlugQueryHandler(mockRepository.Object);
            var query = new GetPublishedStoreFrontBySlugQuery(slugValue);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(tenantId, result.TenantId);
            Assert.AreEqual(storeName, result.Name);
            Assert.AreEqual(slugValue, result.Slug);
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
            var slug = Slug.Create("valid-slug");
            var cancellationToken = new CancellationToken(canceled: true);

            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Stores.Store?)null);

            var handler = new GetPublishedStoreFrontBySlugQueryHandler(mockRepository.Object);
            var query = new GetPublishedStoreFrontBySlugQuery("valid-slug");

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            mockRepository.Verify(
                r => r.GetBySlugAsync(It.IsAny<Slug>(), cancellationToken),
                Times.Once);
        }

        /// <summary>
        /// Tests that Handle correctly handles various valid slug formats with numbers and hyphens.
        /// </summary>
        /// <param name="validSlug">The valid slug string to test.</param>
        [TestMethod]
        [DataRow("simple")]
        [DataRow("with-dash")]
        [DataRow("multiple-dashes-here")]
        [DataRow("with123numbers")]
        [DataRow("123numbers")]
        [DataRow("numbers123")]
        [DataRow("a")]
        [DataRow("1")]
        [DataRow("a-1-b-2")]
        public async Task Handle_ValidSlugFormats_ProcessesSuccessfully(string validSlug)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create(validSlug);
            var store = Domain.Stores.Store.Create(tenantId, "Test Store", slug);
            store.Activate();
            store.Publish();

            var mockRepository = new Mock<IStoreRepository>();
            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var handler = new GetPublishedStoreFrontBySlugQueryHandler(mockRepository.Object);
            var query = new GetPublishedStoreFrontBySlugQuery(validSlug);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(validSlug, result.Slug);
        }

        /// <summary>
        /// Tests that Handle trims whitespace from slug before processing.
        /// </summary>
        [TestMethod]
        public async Task Handle_SlugWithLeadingAndTrailingWhitespace_TrimsAndProcesses()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slugValue = "test-store";
            var slug = Slug.Create(slugValue);
            var store = Domain.Stores.Store.Create(tenantId, "Test Store", slug);
            store.Activate();
            store.Publish();

            var mockRepository = new Mock<IStoreRepository>();
            mockRepository
                .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(store);

            var handler = new GetPublishedStoreFrontBySlugQueryHandler(mockRepository.Object);
            var query = new GetPublishedStoreFrontBySlugQuery("  test-store  ");
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(slugValue, result.Slug);
        }

        /// <summary>
        /// Tests that the constructor successfully initializes the handler when a valid IStoreRepository is provided.
        /// Input: Valid mock IStoreRepository instance.
        /// Expected: Handler is constructed successfully without throwing exceptions.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidStoreRepository_InitializesSuccessfully()
        {
            // Arrange
            var mockStoreRepository = new Mock<IStoreRepository>();

            // Act
            var handler = new GetPublishedStoreFrontBySlugQueryHandler(mockStoreRepository.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

    }
}