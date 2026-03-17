using System;
using System.Threading;
using System.Threading.Tasks;

using Moq;
using Store.Application.Abstractions;
using Store.Application.Exceptions;
using Store.Application.Stores.Commands.UnpublishStore;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Commands.UnpublishStore.UnitTests
{
    [TestClass]
    public sealed class UnpublishStoreCommandHandlerTests
    {
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var repository = new Mock<IStoreRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();

            var handler = new UnpublishStoreCommandHandler(repository.Object, unitOfWork.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task Handle_StoreNotFound_ThrowsStoreNotFoundException()
        {
            var tenantId = Guid.NewGuid();
            var command = new UnpublishStoreCommand(tenantId);
            var cancellationToken = CancellationToken.None;

            var repository = new Mock<IStoreRepository>();
            repository.Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken)).ReturnsAsync((Store.Domain.Stores.Store?)null);

            var unitOfWork = new Mock<IUnitOfWork>();
            var handler = new UnpublishStoreCommandHandler(repository.Object, unitOfWork.Object);

            try
            {
                await handler.Handle(command, cancellationToken);
                Assert.Fail("Expected StoreNotFoundException was not thrown.");
            }
            catch (StoreNotFoundException)
            {
                // expected
            }

            repository.Verify(r => r.GetByTenantIdAsync(tenantId, cancellationToken), Times.Once);
            unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task Handle_StoreExists_UnpublishesAndSavesChanges()
        {
            var tenantId = Guid.NewGuid();
            var command = new UnpublishStoreCommand(tenantId);
            var cancellationToken = CancellationToken.None;

            var store = Store.Domain.Stores.Store.Create(tenantId, "Test Store", Slug.Create("test-store"));
            store.Publish();

            var repository = new Mock<IStoreRepository>();
            repository.Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken)).ReturnsAsync(store);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.SaveChangesAsync(cancellationToken)).ReturnsAsync(1);

            var handler = new UnpublishStoreCommandHandler(repository.Object, unitOfWork.Object);

            await handler.Handle(command, cancellationToken);

            Assert.IsFalse(store.IsPublished);
            repository.Verify(r => r.GetByTenantIdAsync(tenantId, cancellationToken), Times.Once);
            unitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
        }

        [TestMethod]
        public async Task Handle_PassesCancellationTokenToDependencies()
        {
            var tenantId = Guid.NewGuid();
            var command = new UnpublishStoreCommand(tenantId);
            var cts = new CancellationTokenSource();

            var store = Store.Domain.Stores.Store.Create(tenantId, "Test Store", Slug.Create("test-store"));

            var repository = new Mock<IStoreRepository>();
            repository.Setup(r => r.GetByTenantIdAsync(tenantId, cts.Token)).ReturnsAsync(store);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.SaveChangesAsync(cts.Token)).ReturnsAsync(1);

            var handler = new UnpublishStoreCommandHandler(repository.Object, unitOfWork.Object);

            await handler.Handle(command, cts.Token);

            repository.Verify(r => r.GetByTenantIdAsync(tenantId, cts.Token), Times.Once);
            unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
        }
    }
}
