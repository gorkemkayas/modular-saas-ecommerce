using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application.Abstractions;
using Store.Application.Exceptions;
using Store.Application.Stores.Commands.PublishStore;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Commands.PublishStore.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="PublishStoreCommandHandler"/> class.
    /// </summary>
    [TestClass]
    public sealed class PublishStoreCommandHandlerTests
    {
        /// <summary>
        /// Tests that the constructor successfully creates an instance when provided with valid dependencies.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Act
            var handler = new PublishStoreCommandHandler(mockRepository.Object, mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that Handle successfully publishes the store and saves changes when a valid store exists.
        /// Input: Valid command with existing store.
        /// Expected: Store.Publish() is executed and SaveChangesAsync is called.
        /// </summary>
        [TestMethod]
        public async Task Handle_StoreExists_PublishesStoreAndSavesChanges()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new PublishStoreCommand(tenantId);
            var cancellationToken = new CancellationToken();

            var store = Domain.Stores.Store.Create(
                tenantId,
                "Test Store",
                Slug.Create("test-store"));
            store.Activate();

            var mockRepository = new Mock<IStoreRepository>();
            mockRepository
                .Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            var handler = new PublishStoreCommandHandler(mockRepository.Object, mockUnitOfWork.Object);

            // Act
            await handler.Handle(command, cancellationToken);

            // Assert
            mockRepository.Verify(r => r.GetByTenantIdAsync(tenantId, cancellationToken), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
            Assert.IsTrue(store.IsPublished);
        }

        /// <summary>
        /// Tests that Handle correctly passes the CancellationToken to repository and unit of work.
        /// Input: Command with a custom CancellationToken.
        /// Expected: Both GetByTenantIdAsync and SaveChangesAsync are called with the same CancellationToken.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidCommand_PassesCancellationTokenToDependencies()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new PublishStoreCommand(tenantId);
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var store = Domain.Stores.Store.Create(
                tenantId,
                "Test Store",
                Slug.Create("test-store"));
            store.Activate();

            var mockRepository = new Mock<IStoreRepository>();
            mockRepository
                .Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            var handler = new PublishStoreCommandHandler(mockRepository.Object, mockUnitOfWork.Object);

            // Act
            await handler.Handle(command, cancellationToken);

            // Assert
            mockRepository.Verify(r => r.GetByTenantIdAsync(tenantId, cancellationToken), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that Handle calls repository with the exact TenantId from the command.
        /// Input: Command with a specific TenantId.
        /// Expected: GetByTenantIdAsync is called with the same TenantId.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidCommand_CallsRepositoryWithCorrectTenantId()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new PublishStoreCommand(tenantId);
            var cancellationToken = new CancellationToken();

            var store = Domain.Stores.Store.Create(
                tenantId,
                "Test Store",
                Slug.Create("test-store"));
            store.Activate();

            var mockRepository = new Mock<IStoreRepository>();
            mockRepository
                .Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            var handler = new PublishStoreCommandHandler(mockRepository.Object, mockUnitOfWork.Object);

            // Act
            await handler.Handle(command, cancellationToken);

            // Assert
            mockRepository.Verify(r => r.GetByTenantIdAsync(tenantId, cancellationToken), Times.Once);
        }
    }
}