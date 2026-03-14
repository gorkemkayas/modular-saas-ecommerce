using System;
using System.Threading;
using System.Threading.Tasks;

using Moq;
using Store.Application.Abstractions;
using Store.Application.Stores.Queries.GetStoreById;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Queries.GetStoreById.UnitTests
{
    [TestClass]
    public sealed class GetStoreByIdQueryHandlerTests
    {
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var repository = new Mock<IStoreRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();

            var handler = new GetStoreByIdQueryHandler(repository.Object, unitOfWork.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task Handle_StoreNotFound_ReturnsNull()
        {
            var storeId = Guid.NewGuid();
            var query = new GetStoreByIdQuery(storeId);
            var cancellationToken = CancellationToken.None;

            var repository = new Mock<IStoreRepository>();
            repository.Setup(r => r.GetByIdAsync(storeId, cancellationToken)).ReturnsAsync((Store.Domain.Stores.Store?)null);

            var unitOfWork = new Mock<IUnitOfWork>();
            var handler = new GetStoreByIdQueryHandler(repository.Object, unitOfWork.Object);

            var result = await handler.Handle(query, cancellationToken);

            Assert.IsNull(result);
            repository.Verify(r => r.GetByIdAsync(storeId, cancellationToken), Times.Once);
            unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task Handle_StoreExists_MapsAllFieldsToDto()
        {
            var tenantId = Guid.NewGuid();
            var store = Store.Domain.Stores.Store.Create(
                tenantId,
                "Test Store",
                Slug.Create("test-store"),
                "Description",
                "https://logo.example");

            store.Publish();

            var query = new GetStoreByIdQuery(store.Id);
            var cancellationToken = CancellationToken.None;

            var repository = new Mock<IStoreRepository>();
            repository.Setup(r => r.GetByIdAsync(store.Id, cancellationToken)).ReturnsAsync(store);

            var unitOfWork = new Mock<IUnitOfWork>();
            var handler = new GetStoreByIdQueryHandler(repository.Object, unitOfWork.Object);

            var result = await handler.Handle(query, cancellationToken);

            Assert.IsNotNull(result);
            Assert.AreEqual(store.Id, result.Id);
            Assert.AreEqual(store.TenantId, result.TenantId);
            Assert.AreEqual(store.Name, result.Name);
            Assert.AreEqual(store.Slug.Value, result.Slug);
            Assert.AreEqual(store.Description, result.Description);
            Assert.AreEqual(store.LogoUrl, result.LogoUrl);
            Assert.AreEqual(store.Status, result.Status);
            Assert.AreEqual(store.IsPublished, result.IsPublished);

            repository.Verify(r => r.GetByIdAsync(store.Id, cancellationToken), Times.Once);
            unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task Handle_PassesCancellationTokenToRepository()
        {
            var storeId = Guid.NewGuid();
            var query = new GetStoreByIdQuery(storeId);
            var cts = new CancellationTokenSource();

            var repository = new Mock<IStoreRepository>();
            repository.Setup(r => r.GetByIdAsync(storeId, cts.Token)).ReturnsAsync((Store.Domain.Stores.Store?)null);

            var unitOfWork = new Mock<IUnitOfWork>();
            var handler = new GetStoreByIdQueryHandler(repository.Object, unitOfWork.Object);

            await handler.Handle(query, cts.Token);

            repository.Verify(r => r.GetByIdAsync(storeId, cts.Token), Times.Once);
        }
    }
}
