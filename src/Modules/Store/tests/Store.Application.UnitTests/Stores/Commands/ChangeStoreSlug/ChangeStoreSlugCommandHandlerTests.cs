using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application.Abstractions;
using Store.Application.Stores.Commands.ChangeStoreSlug;
using Store.Domain.Stores;


namespace Store.Application.Stores.Commands.ChangeStoreSlug.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="ChangeStoreSlugCommandHandler"/> class.
    /// </summary>
    [TestClass]
    public sealed class ChangeStoreSlugCommandHandlerTests
    {
        /// <summary>
        /// Tests that the constructor successfully creates an instance when provided with valid dependencies.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidParameters_CreatesInstance()
        {
            // Arrange
            var storeRepositoryMock = new Mock<IStoreRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            // Act
            var handler = new ChangeStoreSlugCommandHandler(
                storeRepositoryMock.Object,
                unitOfWorkMock.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor does not throw when storeRepository parameter is null.
        /// This test reveals that the constructor lacks null validation for non-nullable parameters.
        /// </summary>
        [TestMethod]
        public void Constructor_NullStoreRepository_DoesNotThrow()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            // Act & Assert
            var handler = new ChangeStoreSlugCommandHandler(
                null!,
                unitOfWorkMock.Object);

            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor does not throw when unitOfWork parameter is null.
        /// This test reveals that the constructor lacks null validation for non-nullable parameters.
        /// </summary>
        [TestMethod]
        public void Constructor_NullUnitOfWork_DoesNotThrow()
        {
            // Arrange
            var storeRepositoryMock = new Mock<IStoreRepository>();

            // Act & Assert
            var handler = new ChangeStoreSlugCommandHandler(
                storeRepositoryMock.Object,
                null!);

            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor does not throw when both parameters are null.
        /// This test reveals that the constructor lacks null validation for non-nullable parameters.
        /// </summary>
        [TestMethod]
        public void Constructor_BothParametersNull_DoesNotThrow()
        {
            // Act & Assert
            var handler = new ChangeStoreSlugCommandHandler(null!, null!);

            Assert.IsNotNull(handler);
        }
    }
}