using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application.DTOs;
using Store.Application.Stores.Queries.GetStoreByTenantId;
using Store.Domain.Stores;


namespace Store.Application.Stores.Queries.GetStoreByTenantId.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="GetStoreByTenantIdQueryHandler"/> class.
    /// </summary>
    [TestClass]
    public sealed class GetStoreByTenantIdQueryHandlerTests
    {
        /// <summary>
        /// Tests that the constructor successfully creates an instance when provided with a valid repository.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidRepository_CreatesInstance()
        {
            // Arrange
            Mock<IStoreRepository> mockRepository = new Mock<IStoreRepository>();

            // Act
            GetStoreByTenantIdQueryHandler handler = new GetStoreByTenantIdQueryHandler(mockRepository.Object);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetStoreByTenantIdQuery, StoreDto>));
        }
    }
}