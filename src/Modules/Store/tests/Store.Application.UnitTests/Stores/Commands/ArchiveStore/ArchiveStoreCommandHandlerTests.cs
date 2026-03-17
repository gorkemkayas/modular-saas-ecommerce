using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Store.Application.Abstractions;
using Store.Application.Stores.Commands.ArchiveStore;
using Store.Domain.Stores;


namespace Store.Application.Stores.Commands.ArchiveStore.UnitTests
{
    /// <summary>
    /// Unit tests for the ArchiveStoreCommandHandler class.
    /// </summary>
    [TestClass]
    public sealed class ArchiveStoreCommandHandlerTests
    {
        /// <summary>
        /// Tests that the constructor initializes successfully with all valid dependencies.
        /// Input: Valid mock instances for storeRepository, unitOfWork, and logger.
        /// Expected: Constructor completes without throwing an exception.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_InitializesSuccessfully()
        {
            // Arrange
            var storeRepositoryMock = new Mock<IStoreRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<ArchiveStoreCommandHandler>>();

            // Act
            var handler = new ArchiveStoreCommandHandler(
                storeRepositoryMock.Object,
                unitOfWorkMock.Object,
                loggerMock.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor behavior when null is passed for storeRepository parameter.
        /// Input: null for storeRepository, valid mocks for unitOfWork and logger.
        /// Expected: Constructor completes (no validation present in constructor).
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullStoreRepository_DoesNotThrow()
        {
            // Arrange
            IStoreRepository? storeRepository = null;
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<ArchiveStoreCommandHandler>>();

            // Act
            var handler = new ArchiveStoreCommandHandler(
                storeRepository!,
                unitOfWorkMock.Object,
                loggerMock.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor behavior when null is passed for unitOfWork parameter.
        /// Input: Valid mock for storeRepository, null for unitOfWork, valid mock for logger.
        /// Expected: Constructor completes (no validation present in constructor).
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullUnitOfWork_DoesNotThrow()
        {
            // Arrange
            var storeRepositoryMock = new Mock<IStoreRepository>();
            IUnitOfWork? unitOfWork = null;
            var loggerMock = new Mock<ILogger<ArchiveStoreCommandHandler>>();

            // Act
            var handler = new ArchiveStoreCommandHandler(
                storeRepositoryMock.Object,
                unitOfWork!,
                loggerMock.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor behavior when null is passed for logger parameter.
        /// Input: Valid mocks for storeRepository and unitOfWork, null for logger.
        /// Expected: Constructor completes (no validation present in constructor).
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullLogger_DoesNotThrow()
        {
            // Arrange
            var storeRepositoryMock = new Mock<IStoreRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            ILogger<ArchiveStoreCommandHandler>? logger = null;

            // Act
            var handler = new ArchiveStoreCommandHandler(
                storeRepositoryMock.Object,
                unitOfWorkMock.Object,
                logger!);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor behavior when all parameters are null.
        /// Input: null for all three parameters.
        /// Expected: Constructor completes (no validation present in constructor).
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllNullParameters_DoesNotThrow()
        {
            // Arrange
            IStoreRepository? storeRepository = null;
            IUnitOfWork? unitOfWork = null;
            ILogger<ArchiveStoreCommandHandler>? logger = null;

            // Act
            var handler = new ArchiveStoreCommandHandler(
                storeRepository!,
                unitOfWork!,
                logger!);

            // Assert
            Assert.IsNotNull(handler);
        }
    }
}