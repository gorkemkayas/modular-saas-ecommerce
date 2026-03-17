using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application.Abstractions;
using Store.Application.Stores.Commands.ActivateStore;
using Store.Domain.Stores;


namespace Store.Application.Stores.Commands.ActivateStore.UnitTests
{
    [TestClass]
    public sealed class ActivateStoreCommandHandlerTests
    {
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
            var handler = new ActivateStoreCommandHandler(mockStoreRepository.Object, mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests the constructor behavior when storeRepository parameter is null.
        /// Verifies that the constructor accepts null without throwing an exception.
        /// Note: This may indicate missing validation in the production code.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullStoreRepository_DoesNotThrow()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Act
            var handler = new ActivateStoreCommandHandler(null!, mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests the constructor behavior when unitOfWork parameter is null.
        /// Verifies that the constructor accepts null without throwing an exception.
        /// Note: This may indicate missing validation in the production code.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullUnitOfWork_DoesNotThrow()
        {
            // Arrange
            var mockStoreRepository = new Mock<IStoreRepository>();

            // Act
            var handler = new ActivateStoreCommandHandler(mockStoreRepository.Object, null!);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests the constructor behavior when both parameters are null.
        /// Verifies that the constructor accepts null values without throwing an exception.
        /// Note: This may indicate missing validation in the production code.
        /// </summary>
        [TestMethod]
        public void Constructor_WithBothParametersNull_DoesNotThrow()
        {
            // Arrange & Act
            var handler = new ActivateStoreCommandHandler(null!, null!);

            // Assert
            Assert.IsNotNull(handler);
        }
    }
}