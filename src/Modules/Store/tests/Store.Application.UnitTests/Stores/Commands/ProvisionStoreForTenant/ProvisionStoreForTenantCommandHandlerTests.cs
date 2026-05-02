using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Moq;
using Store.Application.Abstractions;
using Store.Application.Exceptions;
using Store.Application.Stores.Commands.ProvisionStoreForTenant;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Commands.ProvisionStoreForTenant.UnitTests
{
    [TestClass]
    public sealed class ProvisionStoreForTenantCommandHandlerTests
    {
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var repository = new Mock<IStoreRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var logger = new Mock<ILogger<ProvisionStoreForTenantCommandHandler>>();

            var handler = new ProvisionStoreForTenantCommandHandler(repository.Object, unitOfWork.Object, logger.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task Handle_TenantAlreadyExists_ThrowsStoreAlreadyExistsForTenantException()
        {
            var tenantId = Guid.NewGuid();
            var command = new ProvisionStoreForTenantCommand(tenantId, "Store Name", "store-slug");
            var cancellationToken = CancellationToken.None;

            var repository = new Mock<IStoreRepository>();
            repository.Setup(r => r.ExistsByTenantIdAsync(tenantId, cancellationToken)).ReturnsAsync(true);

            var unitOfWork = new Mock<IUnitOfWork>();
            var logger = new Mock<ILogger<ProvisionStoreForTenantCommandHandler>>();

            var handler = new ProvisionStoreForTenantCommandHandler(repository.Object, unitOfWork.Object, logger.Object);

            try
            {
                await handler.Handle(command, cancellationToken);
                Assert.Fail("Expected StoreAlreadyExistsForTenantException was not thrown.");
            }
            catch (StoreAlreadyExistsForTenantException)
            {
                // expected
            }

            repository.Verify(r => r.ExistsByTenantIdAsync(tenantId, cancellationToken), Times.Once);
            repository.Verify(r => r.ExistsBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()), Times.Never);
            repository.Verify(r => r.AddAsync(It.IsAny<Store.Domain.Stores.Store>(), It.IsAny<CancellationToken>()), Times.Never);
            unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task Handle_SlugAlreadyExists_ThrowsDuplicateStoreSlugException()
        {
            var tenantId = Guid.NewGuid();
            var command = new ProvisionStoreForTenantCommand(tenantId, "Store Name", "store-slug");
            var cancellationToken = CancellationToken.None;

            var repository = new Mock<IStoreRepository>();
            repository.Setup(r => r.ExistsByTenantIdAsync(tenantId, cancellationToken)).ReturnsAsync(false);
            repository.Setup(r => r.ExistsBySlugAsync(It.Is<Slug>(s => s.Value == "store-slug"), cancellationToken)).ReturnsAsync(true);

            var unitOfWork = new Mock<IUnitOfWork>();
            var logger = new Mock<ILogger<ProvisionStoreForTenantCommandHandler>>();

            var handler = new ProvisionStoreForTenantCommandHandler(repository.Object, unitOfWork.Object, logger.Object);

            try
            {
                await handler.Handle(command, cancellationToken);
                Assert.Fail("Expected DuplicateStoreSlugException was not thrown.");
            }
            catch (DuplicateStoreSlugException)
            {
                // expected
            }

            repository.Verify(r => r.ExistsByTenantIdAsync(tenantId, cancellationToken), Times.Once);
            repository.Verify(r => r.ExistsBySlugAsync(It.Is<Slug>(s => s.Value == "store-slug"), cancellationToken), Times.Once);
            repository.Verify(r => r.AddAsync(It.IsAny<Store.Domain.Stores.Store>(), It.IsAny<CancellationToken>()), Times.Never);
            unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task Handle_ValidCommand_AddsStore_SavesAndReturnsStoreId()
        {
            var tenantId = Guid.NewGuid();
            var command = new ProvisionStoreForTenantCommand(tenantId, "Store Name", "store-slug");
            var cancellationToken = CancellationToken.None;

            Store.Domain.Stores.Store? addedStore = null;

            var repository = new Mock<IStoreRepository>();
            repository.Setup(r => r.ExistsByTenantIdAsync(tenantId, cancellationToken)).ReturnsAsync(false);
            repository.Setup(r => r.ExistsBySlugAsync(It.Is<Slug>(s => s.Value == "store-slug"), cancellationToken)).ReturnsAsync(false);
            repository
                .Setup(r => r.AddAsync(It.IsAny<Store.Domain.Stores.Store>(), cancellationToken))
                .Callback<Store.Domain.Stores.Store, CancellationToken>((store, _) => addedStore = store)
                .Returns(Task.CompletedTask);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.SaveChangesAsync(cancellationToken)).ReturnsAsync(1);

            var logger = new Mock<ILogger<ProvisionStoreForTenantCommandHandler>>();

            var handler = new ProvisionStoreForTenantCommandHandler(repository.Object, unitOfWork.Object, logger.Object);

            var result = await handler.Handle(command, cancellationToken);

            Assert.IsNotNull(addedStore);
            Assert.AreEqual(tenantId, addedStore.TenantId);
            Assert.AreEqual("Store Name", addedStore.Name);
            Assert.AreEqual("store-slug", addedStore.Slug.Value);
            Assert.AreEqual(addedStore.Id, result);

            repository.Verify(r => r.AddAsync(It.IsAny<Store.Domain.Stores.Store>(), cancellationToken), Times.Once);
            unitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
        }
    }
}
