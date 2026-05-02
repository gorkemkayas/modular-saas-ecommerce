using BuildingBlocks.Application.Abstractions.Tenancy;
using ECommerce.API.Contracts.Store.UpdateStoreProfile;
using ECommerce.API.Controllers.Store;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application.DTOs;
using Store.Application.Stores.Commands.ChangeStoreSlug;
using Store.Application.Stores.Commands.ProvisionStoreForTenant;
using Store.Application.Stores.Commands.PublishStore;
using Store.Application.Stores.Commands.UnpublishStore;
using Store.Application.Stores.Commands.UpdateStoreProfile;
using Store.Application.Stores.Queries.CheckStoreSlugAvailability;
using Store.Application.Stores.Queries.GetPublishedStorefrontBySlug;
using Store.Application.Stores.Queries.GetStoreByTenantId;
using Store.Application.Stores.Queries.SuggestAvailableSlug;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.API.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the StoresController class.
    /// </summary>
    [TestClass]
    public class StoresControllerTests
    {
        /// <summary>
        /// Tests that GetStore returns NotFoundResult when the store does not exist.
        /// Input: Valid tenant ID but store not found (null result from sender).
        /// Expected: Returns NotFoundResult.
        /// </summary>
        [TestMethod]
        public async Task GetStore_WhenStoreDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<GetStoreByTenantIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync((StoreDto? )null);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var cancellationToken = CancellationToken.None;
            // Act
            var result = await controller.GetStore(cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests that UnpublishStore with a valid tenant ID sends the command and returns NoContent (204).
        /// Input: Valid Guid tenant ID.
        /// Expected: UnpublishStoreCommand is sent via ISender and NoContentResult is returned.
        /// </summary>
        [TestMethod]
        public async Task UnpublishStore_ValidTenantId_SendsCommandAndReturnsNoContent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<UnpublishStoreCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var cancellationToken = CancellationToken.None;
            // Act
            var result = await controller.UnpublishStore(cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            mockSender.Verify(x => x.Send(It.Is<UnpublishStoreCommand>(cmd => cmd.TenantId == tenantId), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that UnpublishStore with Guid.Empty tenant ID successfully processes the request.
        /// Input: Guid.Empty as tenant ID.
        /// Expected: UnpublishStoreCommand is sent with Guid.Empty and NoContentResult is returned.
        /// </summary>
        [TestMethod]
        public async Task UnpublishStore_GuidEmptyTenantId_SendsCommandAndReturnsNoContent()
        {
            // Arrange
            var tenantId = Guid.Empty;
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<UnpublishStoreCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var cancellationToken = CancellationToken.None;
            // Act
            var result = await controller.UnpublishStore(cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            mockSender.Verify(x => x.Send(It.Is<UnpublishStoreCommand>(cmd => cmd.TenantId == Guid.Empty), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance when provided with valid dependencies.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidParameters_CreatesInstance()
        {
            // Arrange
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            // Act
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Assert
            Assert.IsNotNull(controller);
            Assert.IsInstanceOfType(controller, typeof(StoresController));
        }

        /// <summary>
        /// Tests that the constructor accepts a null sender parameter without throwing an exception.
        /// This documents the actual behavior where no explicit null validation is performed,
        /// even though the parameter is marked as non-nullable.
        /// </summary>
        [TestMethod]
        public void Constructor_NullSender_DoesNotThrowImmediately()
        {
            // Arrange
            ISender? nullSender = null;
            var mockTenantContext = new Mock<ITenantContext>();
            // Act
            var controller = new StoresController(nullSender!, mockTenantContext.Object);
            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that the constructor accepts a null tenant context parameter without throwing an exception.
        /// This documents the actual behavior where no explicit null validation is performed,
        /// even though the parameter is marked as non-nullable.
        /// </summary>
        [TestMethod]
        public void Constructor_NullTenantContext_DoesNotThrowImmediately()
        {
            // Arrange
            var mockSender = new Mock<ISender>();
            ITenantContext? nullTenantContext = null;
            // Act
            var controller = new StoresController(mockSender.Object, nullTenantContext!);
            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that the constructor accepts both null parameters without throwing an exception.
        /// This documents the actual behavior where no explicit null validation is performed,
        /// even though both parameters are marked as non-nullable.
        /// </summary>
        [TestMethod]
        public void Constructor_BothParametersNull_DoesNotThrowImmediately()
        {
            // Arrange
            ISender? nullSender = null;
            ITenantContext? nullTenantContext = null;
            // Act
            var controller = new StoresController(nullSender!, nullTenantContext!);
            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that ChangeStoreSlug sends the correct command with valid inputs and returns NoContent result.
        /// </summary>
        [TestMethod]
        public async Task ChangeStoreSlug_ValidSlug_SendsCommandAndReturnsNoContent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var newSlug = "my-new-store-slug";
            var cancellationToken = CancellationToken.None;
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<ChangeStoreSlugCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.ChangeStoreSlug(newSlug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<NoContentResult>(result);
            mockSender.Verify(x => x.Send(It.Is<ChangeStoreSlugCommand>(cmd => cmd.TenantId == tenantId && cmd.NewSlug == newSlug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that ChangeStoreSlug with empty string slug sends the command as-is and returns NoContent.
        /// </summary>
        [TestMethod]
        public async Task ChangeStoreSlug_EmptyStringSlug_SendsCommandAndReturnsNoContent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var newSlug = string.Empty;
            var cancellationToken = CancellationToken.None;
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<ChangeStoreSlugCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.ChangeStoreSlug(newSlug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<NoContentResult>(result);
            mockSender.Verify(x => x.Send(It.Is<ChangeStoreSlugCommand>(cmd => cmd.NewSlug == string.Empty), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that ChangeStoreSlug with whitespace-only slug sends the command as-is and returns NoContent.
        /// </summary>
        [TestMethod]
        public async Task ChangeStoreSlug_WhitespaceSlug_SendsCommandAndReturnsNoContent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var newSlug = "   ";
            var cancellationToken = CancellationToken.None;
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<ChangeStoreSlugCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.ChangeStoreSlug(newSlug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<NoContentResult>(result);
            mockSender.Verify(x => x.Send(It.Is<ChangeStoreSlugCommand>(cmd => cmd.NewSlug == "   "), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that ChangeStoreSlug with special characters slug sends the command correctly.
        /// </summary>
        [TestMethod]
        public async Task ChangeStoreSlug_SlugWithSpecialCharacters_SendsCommandAndReturnsNoContent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var newSlug = "slug-with-special!@#$%^&*()";
            var cancellationToken = CancellationToken.None;
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<ChangeStoreSlugCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.ChangeStoreSlug(newSlug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<NoContentResult>(result);
            mockSender.Verify(x => x.Send(It.Is<ChangeStoreSlugCommand>(cmd => cmd.NewSlug == newSlug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that ChangeStoreSlug with a very long slug sends the command correctly.
        /// </summary>
        [TestMethod]
        public async Task ChangeStoreSlug_VeryLongSlug_SendsCommandAndReturnsNoContent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var newSlug = new string ('a', 10000);
            var cancellationToken = CancellationToken.None;
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<ChangeStoreSlugCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.ChangeStoreSlug(newSlug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<NoContentResult>(result);
            mockSender.Verify(x => x.Send(It.Is<ChangeStoreSlugCommand>(cmd => cmd.NewSlug == newSlug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that ChangeStoreSlug with single character slug sends the command correctly.
        /// </summary>
        [TestMethod]
        public async Task ChangeStoreSlug_SingleCharacterSlug_SendsCommandAndReturnsNoContent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var newSlug = "a";
            var cancellationToken = CancellationToken.None;
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<ChangeStoreSlugCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.ChangeStoreSlug(newSlug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<NoContentResult>(result);
            mockSender.Verify(x => x.Send(It.Is<ChangeStoreSlugCommand>(cmd => cmd.NewSlug == newSlug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that ChangeStoreSlug propagates the cancellation token to the Send method.
        /// </summary>
        [TestMethod]
        public async Task ChangeStoreSlug_WithCancellationToken_PropagatesCancellationToken()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var newSlug = "test-slug";
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<ChangeStoreSlugCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.ChangeStoreSlug(newSlug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<NoContentResult>(result);
            mockSender.Verify(x => x.Send(It.IsAny<ChangeStoreSlugCommand>(), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that ChangeStoreSlug correctly uses Guid.Empty as tenant ID when provided.
        /// </summary>
        [TestMethod]
        public async Task ChangeStoreSlug_EmptyGuidTenantId_SendsCommandWithEmptyGuid()
        {
            // Arrange
            var tenantId = Guid.Empty;
            var newSlug = "test-slug";
            var cancellationToken = CancellationToken.None;
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<ChangeStoreSlugCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.ChangeStoreSlug(newSlug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<NoContentResult>(result);
            mockSender.Verify(x => x.Send(It.Is<ChangeStoreSlugCommand>(cmd => cmd.TenantId == Guid.Empty), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that ChangeStoreSlug with slug containing Unicode characters sends the command correctly.
        /// </summary>
        [TestMethod]
        public async Task ChangeStoreSlug_UnicodeSlug_SendsCommandAndReturnsNoContent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var newSlug = "店铺-مخزن-🏪";
            var cancellationToken = CancellationToken.None;
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<ChangeStoreSlugCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.ChangeStoreSlug(newSlug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<NoContentResult>(result);
            mockSender.Verify(x => x.Send(It.Is<ChangeStoreSlugCommand>(cmd => cmd.NewSlug == newSlug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that ChangeStoreSlug with control characters in slug sends the command correctly.
        /// </summary>
        [TestMethod]
        public async Task ChangeStoreSlug_SlugWithControlCharacters_SendsCommandAndReturnsNoContent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var newSlug = "slug\t\n\r\0test";
            var cancellationToken = CancellationToken.None;
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            mockSender.Setup(x => x.Send(It.IsAny<ChangeStoreSlugCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.ChangeStoreSlug(newSlug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<NoContentResult>(result);
            mockSender.Verify(x => x.Send(It.Is<ChangeStoreSlugCommand>(cmd => cmd.NewSlug == newSlug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that PublishStore sends the correct command with valid TenantIdAsGuid and returns NoContent result.
        /// Input: Valid Guid from TenantIdAsGuid and CancellationToken.None.
        /// Expected: Command sent to ISender with correct Guid, method returns NoContentResult.
        /// </summary>
        [TestMethod]
        public async Task PublishStore_ValidTenantId_SendsCommandAndReturnsNoContent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var cancellationToken = CancellationToken.None;
            // Act
            var result = await controller.PublishStore(cancellationToken);
            // Assert
            mockSender.Verify(x => x.Send(It.Is<PublishStoreCommand>(cmd => cmd.TenantId == tenantId), cancellationToken), Times.Once);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        /// <summary>
        /// Tests that PublishStore correctly propagates the cancellation token to the sender.
        /// Input: Valid TenantIdAsGuid and a custom CancellationToken.
        /// Expected: The exact cancellation token is passed to ISender.Send.
        /// </summary>
        [TestMethod]
        public async Task PublishStore_WithCancellationToken_PropagatesTokenToSender()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            // Act
            await controller.PublishStore(cancellationToken);
            // Assert
            mockSender.Verify(x => x.Send(It.IsAny<PublishStoreCommand>(), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that PublishStore works correctly with different valid Guid values including edge case Guids.
        /// Input: Various valid Guid values (Guid.Empty, random Guid, max Guid).
        /// Expected: Command sent with correct Guid value and NoContent returned for each.
        /// </summary>
        [TestMethod]
        [DataRow("00000000-0000-0000-0000-000000000000", DisplayName = "Empty Guid")]
        [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff", DisplayName = "Max Guid")]
        [DataRow("12345678-1234-1234-1234-123456789abc", DisplayName = "Standard Guid")]
        public async Task PublishStore_DifferentValidGuids_SendsCorrectCommandAndReturnsNoContent(string guidString)
        {
            // Arrange
            var tenantId = Guid.Parse(guidString);
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var cancellationToken = CancellationToken.None;
            // Act
            var result = await controller.PublishStore(cancellationToken);
            // Assert
            mockSender.Verify(x => x.Send(It.Is<PublishStoreCommand>(cmd => cmd.TenantId == tenantId), cancellationToken), Times.Once);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        /// <summary>
        /// Tests that PublishStore returns NoContentResult after successful command execution.
        /// Input: Valid tenant ID and successful Send operation.
        /// Expected: Returns NoContentResult (204 status).
        /// </summary>
        [TestMethod]
        public async Task PublishStore_SuccessfulExecution_ReturnsNoContentResult()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var mockSender = new Mock<ISender>();
            mockSender.Setup(x => x.Send(It.IsAny<PublishStoreCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.PublishStore(CancellationToken.None);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        /// <summary>
        /// Tests CheckStoreSlugAvailability method with a valid slug that is available.
        /// Verifies that the method returns 200 OK with correct response structure and IsAvailable is true.
        /// </summary>
        [TestMethod]
        public async Task CheckStoreSlugAvailability_ValidSlugAvailable_ReturnsOkWithTrueAvailability()
        {
            // Arrange
            const string slug = "valid-slug";
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken)).ReturnsAsync(true);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.CheckStoreSlugAvailability(slug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value;
            Assert.IsNotNull(response);
            var slugProperty = response.GetType().GetProperty("Slug");
            var isAvailableProperty = response.GetType().GetProperty("IsAvailable");
            Assert.IsNotNull(slugProperty);
            Assert.IsNotNull(isAvailableProperty);
            Assert.AreEqual(slug, slugProperty.GetValue(response));
            Assert.AreEqual(true, isAvailableProperty.GetValue(response));
            mockSender.Verify(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests CheckStoreSlugAvailability method with a valid slug that is not available.
        /// Verifies that the method returns 200 OK with correct response structure and IsAvailable is false.
        /// </summary>
        [TestMethod]
        public async Task CheckStoreSlugAvailability_ValidSlugNotAvailable_ReturnsOkWithFalseAvailability()
        {
            // Arrange
            const string slug = "taken-slug";
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken)).ReturnsAsync(false);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.CheckStoreSlugAvailability(slug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value;
            Assert.IsNotNull(response);
            var slugProperty = response.GetType().GetProperty("Slug");
            var isAvailableProperty = response.GetType().GetProperty("IsAvailable");
            Assert.IsNotNull(slugProperty);
            Assert.IsNotNull(isAvailableProperty);
            Assert.AreEqual(slug, slugProperty.GetValue(response));
            Assert.AreEqual(false, isAvailableProperty.GetValue(response));
            mockSender.Verify(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests CheckStoreSlugAvailability method with an empty string slug.
        /// Verifies that the method still processes the request and returns the empty slug in the response.
        /// </summary>
        [TestMethod]
        public async Task CheckStoreSlugAvailability_EmptySlug_ReturnsOkWithEmptySlugInResponse()
        {
            // Arrange
            const string slug = "";
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken)).ReturnsAsync(true);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.CheckStoreSlugAvailability(slug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value;
            Assert.IsNotNull(response);
            var responseType = response.GetType();
            var slugProperty = responseType.GetProperty("Slug");
            var isAvailableProperty = responseType.GetProperty("IsAvailable");
            Assert.IsNotNull(slugProperty);
            Assert.IsNotNull(isAvailableProperty);
            var slugValue = slugProperty.GetValue(response) as string;
            var isAvailableValue = (bool)isAvailableProperty.GetValue(response);
            Assert.AreEqual(slug, slugValue);
            Assert.AreEqual(true, isAvailableValue);
            mockSender.Verify(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests CheckStoreSlugAvailability method with a whitespace-only slug.
        /// Verifies that the method processes the request and returns the whitespace slug in the response.
        /// </summary>
        [TestMethod]
        public async Task CheckStoreSlugAvailability_WhitespaceSlug_ReturnsOkWithWhitespaceSlugInResponse()
        {
            // Arrange
            const string slug = "   ";
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken)).ReturnsAsync(false);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.CheckStoreSlugAvailability(slug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Value);
            var slugProperty = okResult.Value.GetType().GetProperty("Slug");
            var isAvailableProperty = okResult.Value.GetType().GetProperty("IsAvailable");
            Assert.IsNotNull(slugProperty);
            Assert.IsNotNull(isAvailableProperty);
            Assert.AreEqual(slug, slugProperty.GetValue(okResult.Value));
            Assert.AreEqual(false, isAvailableProperty.GetValue(okResult.Value));
            mockSender.Verify(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests CheckStoreSlugAvailability method with a slug containing special characters.
        /// Verifies that the method processes the request and correctly handles special characters in the slug.
        /// </summary>
        [TestMethod]
        public async Task CheckStoreSlugAvailability_SlugWithSpecialCharacters_ReturnsOkWithSpecialCharactersInResponse()
        {
            // Arrange
            const string slug = "slug-with-special!@#$%^&*()";
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken)).ReturnsAsync(true);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.CheckStoreSlugAvailability(slug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value;
            Assert.IsNotNull(response);
            var responseType = response.GetType();
            var slugProperty = responseType.GetProperty("Slug");
            var isAvailableProperty = responseType.GetProperty("IsAvailable");
            Assert.IsNotNull(slugProperty);
            Assert.IsNotNull(isAvailableProperty);
            Assert.AreEqual(slug, slugProperty.GetValue(response));
            Assert.AreEqual(true, isAvailableProperty.GetValue(response));
            mockSender.Verify(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests CheckStoreSlugAvailability method with a very long slug string.
        /// Verifies that the method can handle long strings and returns them correctly in the response.
        /// </summary>
        [TestMethod]
        public async Task CheckStoreSlugAvailability_VeryLongSlug_ReturnsOkWithLongSlugInResponse()
        {
            // Arrange
            var slug = new string ('a', 10000);
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken)).ReturnsAsync(false);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.CheckStoreSlugAvailability(slug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value;
            Assert.IsNotNull(response);
            var responseType = response.GetType();
            var slugProperty = responseType.GetProperty("Slug");
            var isAvailableProperty = responseType.GetProperty("IsAvailable");
            Assert.IsNotNull(slugProperty);
            Assert.IsNotNull(isAvailableProperty);
            var actualSlug = slugProperty.GetValue(response) as string;
            var actualIsAvailable = (bool)isAvailableProperty.GetValue(response);
            Assert.AreEqual(slug, actualSlug);
            Assert.AreEqual(false, actualIsAvailable);
            mockSender.Verify(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests CheckStoreSlugAvailability method to ensure the cancellation token is correctly passed to the sender.
        /// Verifies that the cancellation token is propagated through the async call chain.
        /// </summary>
        [TestMethod]
        public async Task CheckStoreSlugAvailability_WithCancellationToken_PassesCancellationTokenToSender()
        {
            // Arrange
            const string slug = "test-slug";
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            mockSender.Setup(s => s.Send(It.IsAny<CheckStoreSlugAvailabilityQuery>(), cancellationToken)).ReturnsAsync(true);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.CheckStoreSlugAvailability(slug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            mockSender.Verify(s => s.Send(It.IsAny<CheckStoreSlugAvailabilityQuery>(), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests CheckStoreSlugAvailability method with various valid slug formats using parameterized test data.
        /// Verifies that the method correctly handles different valid slug patterns and returns appropriate availability status.
        /// </summary>
        /// <param name = "slug">The slug to test.</param>
        /// <param name = "isAvailable">The expected availability status.</param>
        [TestMethod]
        [DataRow("valid-slug", true)]
        [DataRow("slug123", false)]
        [DataRow("my-store-2024", true)]
        [DataRow("a", false)]
        [DataRow("slug_with_underscore", true)]
        [DataRow("UPPERCASE-SLUG", false)]
        [DataRow("slug.with.dots", true)]
        public async Task CheckStoreSlugAvailability_VariousValidSlugs_ReturnsCorrectAvailabilityStatus(string slug, bool isAvailable)
        {
            // Arrange
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken)).ReturnsAsync(isAvailable);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            // Act
            var result = await controller.CheckStoreSlugAvailability(slug, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value;
            Assert.IsNotNull(response);
            var slugProperty = response.GetType().GetProperty("Slug");
            var isAvailableProperty = response.GetType().GetProperty("IsAvailable");
            Assert.IsNotNull(slugProperty);
            Assert.IsNotNull(isAvailableProperty);
            Assert.AreEqual(slug, slugProperty.GetValue(response));
            Assert.AreEqual(isAvailable, isAvailableProperty.GetValue(response));
            mockSender.Verify(s => s.Send(It.Is<CheckStoreSlugAvailabilityQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateStoreProfile sends the correct command to the sender and returns NoContent when all fields are provided.
        /// </summary>
        [TestMethod]
        public async Task UpdateStoreProfile_ValidRequestWithAllFields_SendsCommandAndReturnsNoContent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var request = new UpdateStoreProfileRequest("Test Store", "Test Description", "https://example.com/logo.png");
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(x => x.Send(It.IsAny<UpdateStoreProfileCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));
            // Act
            var result = await controller.UpdateStoreProfile(request, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            mockSender.Verify(x => x.Send(It.Is<UpdateStoreProfileCommand>(cmd => cmd.TenantId == tenantId && cmd.Name == "Test Store" && cmd.Description == "Test Description" && cmd.LogoUrl == "https://example.com/logo.png"), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateStoreProfile correctly handles requests with different optional field combinations.
        /// </summary>
        /// <param name = "name">The store name.</param>
        /// <param name = "description">The store description (optional).</param>
        /// <param name = "logoUrl">The store logo URL (optional).</param>
        [TestMethod]
        [DataRow("Store Name", null, null, DisplayName = "Only name provided")]
        [DataRow("Store Name", "Description", null, DisplayName = "Name and description provided")]
        [DataRow("Store Name", null, "https://example.com/logo.png", DisplayName = "Name and logo URL provided")]
        [DataRow("Store Name", "", "", DisplayName = "Empty strings for optional fields")]
        public async Task UpdateStoreProfile_ValidRequestWithDifferentFieldCombinations_SendsCommandAndReturnsNoContent(string name, string? description, string? logoUrl)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var request = new UpdateStoreProfileRequest(name, description, logoUrl);
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(x => x.Send(It.IsAny<UpdateStoreProfileCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));
            // Act
            var result = await controller.UpdateStoreProfile(request, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            mockSender.Verify(x => x.Send(It.Is<UpdateStoreProfileCommand>(cmd => cmd.TenantId == tenantId && cmd.Name == name && cmd.Description == description && cmd.LogoUrl == logoUrl), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateStoreProfile handles edge case string values for the name field.
        /// </summary>
        /// <param name = "name">The store name with edge case values.</param>
        [TestMethod]
        [DataRow("", DisplayName = "Empty name")]
        [DataRow("   ", DisplayName = "Whitespace only name")]
        [DataRow("Name with special chars: !@#$%^&*()", DisplayName = "Name with special characters")]
        public async Task UpdateStoreProfile_EdgeCaseNameValues_SendsCommandWithProvidedValue(string name)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var request = new UpdateStoreProfileRequest(name, null, null);
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(x => x.Send(It.IsAny<UpdateStoreProfileCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));
            // Act
            var result = await controller.UpdateStoreProfile(request, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            mockSender.Verify(x => x.Send(It.Is<UpdateStoreProfileCommand>(cmd => cmd.Name == name), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateStoreProfile handles very long string values for all fields.
        /// </summary>
        [TestMethod]
        public async Task UpdateStoreProfile_VeryLongStrings_SendsCommandWithLongValues()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var longName = new string ('A', 10000);
            var longDescription = new string ('B', 10000);
            var longLogoUrl = "https://example.com/" + new string ('C', 10000);
            var request = new UpdateStoreProfileRequest(longName, longDescription, longLogoUrl);
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(x => x.Send(It.IsAny<UpdateStoreProfileCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));
            // Act
            var result = await controller.UpdateStoreProfile(request, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            mockSender.Verify(x => x.Send(It.Is<UpdateStoreProfileCommand>(cmd => cmd.Name == longName && cmd.Description == longDescription && cmd.LogoUrl == longLogoUrl), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateStoreProfile correctly retrieves and uses the TenantId from the tenant context.
        /// </summary>
        [TestMethod]
        public async Task UpdateStoreProfile_TenantIdFromContext_UsesCorrectTenantId()
        {
            // Arrange
            var expectedTenantId = Guid.NewGuid();
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(expectedTenantId);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var request = new UpdateStoreProfileRequest("Store Name", null, null);
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(x => x.Send(It.IsAny<UpdateStoreProfileCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));
            // Act
            await controller.UpdateStoreProfile(request, cancellationToken);
            // Assert
            mockTenantContext.Verify(x => x.TenantIdAsGuid, Times.Once);
            mockSender.Verify(x => x.Send(It.Is<UpdateStoreProfileCommand>(cmd => cmd.TenantId == expectedTenantId), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateStoreProfile correctly passes the cancellation token to the sender.
        /// </summary>
        [TestMethod]
        public async Task UpdateStoreProfile_CancellationToken_PassedToSender()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var request = new UpdateStoreProfileRequest("Store Name", null, null);
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            mockSender.Setup(x => x.Send(It.IsAny<UpdateStoreProfileCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));
            // Act
            await controller.UpdateStoreProfile(request, cancellationToken);
            // Assert
            mockSender.Verify(x => x.Send(It.IsAny<UpdateStoreProfileCommand>(), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateStoreProfile handles URL with special characters in LogoUrl field.
        /// </summary>
        [TestMethod]
        [DataRow("https://example.com/logo with spaces.png", DisplayName = "URL with spaces")]
        [DataRow("https://example.com/logo?param=value&other=123", DisplayName = "URL with query parameters")]
        [DataRow("https://example.com/logo#fragment", DisplayName = "URL with fragment")]
        [DataRow("ftp://example.com/logo.png", DisplayName = "FTP protocol")]
        public async Task UpdateStoreProfile_DifferentLogoUrlFormats_SendsCommandWithProvidedUrl(string logoUrl)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var mockSender = new Mock<ISender>();
            var mockTenantContext = new Mock<ITenantContext>();
            mockTenantContext.Setup(x => x.TenantIdAsGuid).Returns(tenantId);
            var controller = new StoresController(mockSender.Object, mockTenantContext.Object);
            var request = new UpdateStoreProfileRequest("Store Name", null, logoUrl);
            var cancellationToken = CancellationToken.None;
            mockSender.Setup(x => x.Send(It.IsAny<UpdateStoreProfileCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));
            // Act
            var result = await controller.UpdateStoreProfile(request, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            mockSender.Verify(x => x.Send(It.Is<UpdateStoreProfileCommand>(cmd => cmd.LogoUrl == logoUrl), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that SuggestAvailableSlug correctly formats the response
        /// with the Slug property containing the SlugSuggestionDto.
        /// </summary>
        [TestMethod]
        public async Task SuggestAvailableSlug_ValidSlug_ReturnsCorrectResponseStructure()
        {
            // Arrange
            string inputSlug = "test-store";
            string suggestedSlugValue = "test-store-available";
            SlugSuggestionDto expectedSuggestion = new SlugSuggestionDto(suggestedSlugValue);
            Mock<ISender> senderMock = new Mock<ISender>();
            Mock<ITenantContext> tenantContextMock = new Mock<ITenantContext>();
            senderMock.Setup(s => s.Send(It.IsAny<SuggestAvailableSlugQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedSuggestion);
            StoresController controller = new StoresController(senderMock.Object, tenantContextMock.Object);
            CancellationToken cancellationToken = CancellationToken.None;
            // Act
            IActionResult result = await controller.SuggestAvailableSlug(inputSlug, cancellationToken);
            // Assert
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Type valueType = okResult.Value!.GetType();
            System.Reflection.PropertyInfo? slugProperty = valueType.GetProperty("Slug");
            Assert.IsNotNull(slugProperty);
            object? slugValue = slugProperty.GetValue(okResult.Value);
            Assert.IsInstanceOfType<SlugSuggestionDto>(slugValue);
            Assert.AreEqual(expectedSuggestion, slugValue);
        }

    }
}
