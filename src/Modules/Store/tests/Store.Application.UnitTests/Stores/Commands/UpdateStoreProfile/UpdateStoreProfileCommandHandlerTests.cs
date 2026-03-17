using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application.Abstractions;
using Store.Application.Exceptions;
using Store.Application.Stores.Commands.UpdateStoreProfile;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Commands.UpdateStoreProfile.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="UpdateStoreProfileCommandHandler"/>.
    /// </summary>
    [TestClass]
    public sealed class UpdateStoreProfileCommandHandlerTests
    {
        private Mock<IStoreRepository> _mockStoreRepository = null!;
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private UpdateStoreProfileCommandHandler _handler = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockStoreRepository = new Mock<IStoreRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _handler = new UpdateStoreProfileCommandHandler(_mockStoreRepository.Object, _mockUnitOfWork.Object);
        }

        /// <summary>
        /// Tests that Handle successfully updates store profile when store exists.
        /// Input: Valid command with store that exists in repository.
        /// Expected: UpdateProfile is called on store and changes are saved.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidCommandWithExistingStore_UpdatesProfileAndSavesChanges()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new UpdateStoreProfileCommand(tenantId, "Updated Store Name", "Updated Description", "https://example.com/logo.png");
            var cancellationToken = CancellationToken.None;

            // Note: Creating a Store instance requires a Slug. Adjust the Slug.Create call based on your actual Slug implementation.
            // Common patterns: Slug.Create("store-slug") or new Slug("store-slug")
            var store = Domain.Stores.Store.Create(
                tenantId,
                "Original Store Name",
                Slug.Create("original-store-slug"),
                "Original Description",
                "https://example.com/original-logo.png");

            _mockStoreRepository
                .Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockStoreRepository.Verify(r => r.GetByTenantIdAsync(tenantId, cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
            Assert.AreEqual("Updated Store Name", store.Name);
            Assert.AreEqual("Updated Description", store.Description);
            Assert.AreEqual("https://example.com/logo.png", store.LogoUrl);
        }

        /// <summary>
        /// Tests that Handle successfully updates store profile with null description.
        /// Input: Valid command with null description.
        /// Expected: Store profile is updated with null description and changes are saved.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidCommandWithNullDescription_UpdatesProfileSuccessfully()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new UpdateStoreProfileCommand(tenantId, "Store Name", null, "https://example.com/logo.png");
            var cancellationToken = CancellationToken.None;

            var store = Domain.Stores.Store.Create(
                tenantId,
                "Original Name",
                Slug.Create("store-slug"),
                "Original Description",
                null);

            _mockStoreRepository
                .Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
            Assert.AreEqual("Store Name", store.Name);
            Assert.IsNull(store.Description);
            Assert.AreEqual("https://example.com/logo.png", store.LogoUrl);
        }

        /// <summary>
        /// Tests that Handle successfully updates store profile with null logo URL.
        /// Input: Valid command with null logoUrl.
        /// Expected: Store profile is updated with null logoUrl and changes are saved.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidCommandWithNullLogoUrl_UpdatesProfileSuccessfully()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new UpdateStoreProfileCommand(tenantId, "Store Name", "Description", null);
            var cancellationToken = CancellationToken.None;

            var store = Domain.Stores.Store.Create(
                tenantId,
                "Original Name",
                Slug.Create("store-slug"),
                null,
                "https://example.com/old-logo.png");

            _mockStoreRepository
                .Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
            Assert.AreEqual("Store Name", store.Name);
            Assert.AreEqual("Description", store.Description);
            Assert.IsNull(store.LogoUrl);
        }

        /// <summary>
        /// Tests that Handle successfully updates store profile with both null description and logoUrl.
        /// Input: Valid command with null description and null logoUrl.
        /// Expected: Store profile is updated with null values and changes are saved.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidCommandWithNullDescriptionAndLogoUrl_UpdatesProfileSuccessfully()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new UpdateStoreProfileCommand(tenantId, "Store Name", null, null);
            var cancellationToken = CancellationToken.None;

            var store = Domain.Stores.Store.Create(
                tenantId,
                "Original Name",
                Slug.Create("store-slug"),
                "Old Description",
                "https://example.com/old-logo.png");

            _mockStoreRepository
                .Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
            Assert.AreEqual("Store Name", store.Name);
            Assert.IsNull(store.Description);
            Assert.IsNull(store.LogoUrl);
        }

        /// <summary>
        /// Tests that Handle passes cancellation token to repository correctly.
        /// Input: Command with custom cancellation token.
        /// Expected: The cancellation token is passed through to GetByTenantIdAsync.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithCancellationToken_PassesTokenToRepository()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new UpdateStoreProfileCommand(tenantId, "Store Name", "Description", "https://example.com/logo.png");
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var store = Domain.Stores.Store.Create(
                tenantId,
                "Original Name",
                Slug.Create("store-slug"),
                null,
                null);

            _mockStoreRepository
                .Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockStoreRepository.Verify(r => r.GetByTenantIdAsync(tenantId, cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that Handle passes cancellation token to unit of work correctly.
        /// Input: Command with custom cancellation token and existing store.
        /// Expected: The cancellation token is passed through to SaveChangesAsync.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithCancellationToken_PassesTokenToUnitOfWork()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new UpdateStoreProfileCommand(tenantId, "Store Name", "Description", null);
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var store = Domain.Stores.Store.Create(
                tenantId,
                "Original Name",
                Slug.Create("store-slug"),
                "Old Description",
                null);

            _mockStoreRepository
                .Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that Handle updates profile with strings containing special characters.
        /// Input: Command with name containing special characters.
        /// Expected: Store profile is updated with the special characters and changes are saved.
        /// </summary>
        [TestMethod]
        public async Task Handle_NameWithSpecialCharacters_UpdatesProfileSuccessfully()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var nameWithSpecialChars = "Store & Company's \"Best\" Store!";
            var command = new UpdateStoreProfileCommand(tenantId, nameWithSpecialChars, "Description with <tags>", "https://example.com/logo.png");
            var cancellationToken = CancellationToken.None;

            var store = Domain.Stores.Store.Create(
                tenantId,
                "Original Name",
                Slug.Create("store-slug"),
                null,
                null);

            _mockStoreRepository
                .Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
            Assert.AreEqual(nameWithSpecialChars.Trim(), store.Name);
        }

        /// <summary>
        /// Tests that Handle updates profile with very long strings.
        /// Input: Command with very long name, description, and logoUrl.
        /// Expected: Store profile is updated with the long strings and changes are saved.
        /// </summary>
        [TestMethod]
        public async Task Handle_VeryLongStrings_UpdatesProfileSuccessfully()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var longName = new string('A', 1000);
            var longDescription = new string('B', 5000);
            var longLogoUrl = "https://example.com/" + new string('c', 1000) + ".png";
            var command = new UpdateStoreProfileCommand(tenantId, longName, longDescription, longLogoUrl);
            var cancellationToken = CancellationToken.None;

            var store = Domain.Stores.Store.Create(
                tenantId,
                "Original Name",
                Slug.Create("store-slug"),
                null,
                null);

            _mockStoreRepository
                .Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
            Assert.AreEqual(longName, store.Name);
            Assert.AreEqual(longDescription, store.Description);
            Assert.AreEqual(longLogoUrl, store.LogoUrl);
        }

        /// <summary>
        /// Tests that Handle updates profile with strings containing only whitespace.
        /// Input: Command with name containing whitespace that will be trimmed.
        /// Expected: Store profile is updated with trimmed strings and changes are saved.
        /// </summary>
        [TestMethod]
        public async Task Handle_StringsWithWhitespace_UpdatesProfileWithTrimmedStrings()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new UpdateStoreProfileCommand(tenantId, "  Store Name  ", "  Description  ", "  https://example.com/logo.png  ");
            var cancellationToken = CancellationToken.None;

            var store = Domain.Stores.Store.Create(
                tenantId,
                "Original Name",
                Slug.Create("store-slug"),
                null,
                null);

            _mockStoreRepository
                .Setup(r => r.GetByTenantIdAsync(tenantId, cancellationToken))
                .ReturnsAsync(store);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
            Assert.AreEqual("Store Name", store.Name);
            Assert.AreEqual("Description", store.Description);
            Assert.AreEqual("https://example.com/logo.png", store.LogoUrl);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance when provided with valid dependencies.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_SuccessfullyCreatesInstance()
        {
            // Arrange
            Mock<IStoreRepository> storeRepositoryMock = new Mock<IStoreRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            // Act
            UpdateStoreProfileCommandHandler handler = new UpdateStoreProfileCommandHandler(
                storeRepositoryMock.Object,
                unitOfWorkMock.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor accepts a null storeRepository parameter without throwing an exception during construction.
        /// Note: While nullable reference types indicate this should not be null, no explicit validation exists in the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_NullStoreRepository_DoesNotThrowDuringConstruction()
        {
            // Arrange
            IStoreRepository? storeRepository = null;
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            // Act
            UpdateStoreProfileCommandHandler handler = new UpdateStoreProfileCommandHandler(
                storeRepository!,
                unitOfWorkMock.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor accepts a null unitOfWork parameter without throwing an exception during construction.
        /// Note: While nullable reference types indicate this should not be null, no explicit validation exists in the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_NullUnitOfWork_DoesNotThrowDuringConstruction()
        {
            // Arrange
            Mock<IStoreRepository> storeRepositoryMock = new Mock<IStoreRepository>();
            IUnitOfWork? unitOfWork = null;

            // Act
            UpdateStoreProfileCommandHandler handler = new UpdateStoreProfileCommandHandler(
                storeRepositoryMock.Object,
                unitOfWork!);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor accepts both null parameters without throwing an exception during construction.
        /// Note: While nullable reference types indicate these should not be null, no explicit validation exists in the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_BothParametersNull_DoesNotThrowDuringConstruction()
        {
            // Arrange
            IStoreRepository? storeRepository = null;
            IUnitOfWork? unitOfWork = null;

            // Act
            UpdateStoreProfileCommandHandler handler = new UpdateStoreProfileCommandHandler(
                storeRepository!,
                unitOfWork!);

            // Assert
            Assert.IsNotNull(handler);
        }
    }
}