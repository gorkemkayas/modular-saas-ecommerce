using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application.Abstractions;
using Store.Application.Exceptions;
using Store.Application.Stores.Commands.SuspendStore;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;
using StoreEntity = Store.Domain.Stores.Store;

namespace Store.Application.Stores.Commands.SuspendStore.UnitTests
{
    [TestClass]
    public sealed class SuspendStoreCommandHandlerTests
    {
        /// <summary>
        /// Tests that Handle successfully suspends an existing store and saves changes.
        /// Validates the happy path where the store exists and all operations complete successfully.
        /// </summary>
        [TestMethod]
        public async Task Handle_StoreExists_SuspendsStoreAndSavesChanges()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            SuspendStoreCommand command = new SuspendStoreCommand(tenantId);
            CancellationToken cancellationToken = CancellationToken.None;

            StoreEntity store = StoreEntity.Create(
                tenantId,
                "Test Store",
                Slug.Create("test-store"));

            Mock<IStoreRepository> storeRepositoryMock = new Mock<IStoreRepository>();
            storeRepositoryMock
                .Setup(x => x.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            SuspendStoreCommandHandler handler = new SuspendStoreCommandHandler(
                storeRepositoryMock.Object,
                unitOfWorkMock.Object);

            // Act
            await handler.Handle(command, cancellationToken);

            // Assert
            storeRepositoryMock.Verify(
                x => x.GetByTenantIdAsync(tenantId, cancellationToken),
                Times.Once);
            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(cancellationToken),
                Times.Once);
            Assert.AreEqual(StoreStatus.Suspended, store.Status);
            Assert.IsFalse(store.IsPublished);
        }

        /// <summary>
        /// Tests that Handle propagates the cancellation token correctly to repository and unit of work.
        /// Validates that the cancellation token is passed through the entire operation chain.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithCancellationToken_PropagatesTokenCorrectly()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            SuspendStoreCommand command = new SuspendStoreCommand(tenantId);
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;

            StoreEntity store = StoreEntity.Create(
                tenantId,
                "Test Store",
                Slug.Create("test-store"));

            Mock<IStoreRepository> storeRepositoryMock = new Mock<IStoreRepository>();
            storeRepositoryMock
                .Setup(x => x.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            SuspendStoreCommandHandler handler = new SuspendStoreCommandHandler(
                storeRepositoryMock.Object,
                unitOfWorkMock.Object);

            // Act
            await handler.Handle(command, cancellationToken);

            // Assert
            storeRepositoryMock.Verify(
                x => x.GetByTenantIdAsync(tenantId, cancellationToken),
                Times.Once);
            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(cancellationToken),
                Times.Once);
        }

        /// <summary>
        /// Tests that Handle suspends a published store and unpublishes it.
        /// Validates that suspending a published store also sets IsPublished to false.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithPublishedStore_SuspendsAndUnpublishesStore()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            SuspendStoreCommand command = new SuspendStoreCommand(tenantId);
            CancellationToken cancellationToken = CancellationToken.None;

            StoreEntity store = StoreEntity.Create(
                tenantId,
                "Test Store",
                Slug.Create("test-store"));
            store.Activate();
            store.Publish();

            Mock<IStoreRepository> storeRepositoryMock = new Mock<IStoreRepository>();
            storeRepositoryMock
                .Setup(x => x.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            SuspendStoreCommandHandler handler = new SuspendStoreCommandHandler(
                storeRepositoryMock.Object,
                unitOfWorkMock.Object);

            // Act
            await handler.Handle(command, cancellationToken);

            // Assert
            Assert.AreEqual(StoreStatus.Suspended, store.Status);
            Assert.IsFalse(store.IsPublished);
            Assert.IsNotNull(store.UpdatedAtUtc);
            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(cancellationToken),
                Times.Once);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance when provided with valid dependencies.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var mockStoreRepository = new Mock<IStoreRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Act
            var handler = new SuspendStoreCommandHandler(
                mockStoreRepository.Object,
                mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor accepts a null store repository parameter.
        /// This test documents that the constructor does not perform null validation,
        /// which may lead to NullReferenceException during method execution.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullStoreRepository_DoesNotThrow()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Act
            var handler = new SuspendStoreCommandHandler(
                null!,
                mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor accepts a null unit of work parameter.
        /// This test documents that the constructor does not perform null validation,
        /// which may lead to NullReferenceException during method execution.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullUnitOfWork_DoesNotThrow()
        {
            // Arrange
            var mockStoreRepository = new Mock<IStoreRepository>();

            // Act
            var handler = new SuspendStoreCommandHandler(
                mockStoreRepository.Object,
                null!);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor accepts null for both parameters.
        /// This test documents that the constructor does not perform any null validation,
        /// which may lead to NullReferenceException during method execution.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllNullParameters_DoesNotThrow()
        {
            // Arrange & Act
            var handler = new SuspendStoreCommandHandler(null!, null!);

            // Assert
            Assert.IsNotNull(handler);
        }
    }
}