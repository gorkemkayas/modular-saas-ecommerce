using System;
using System.Threading;
using System.Threading.Tasks;

using ECommerce.API.Controllers.Store;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application.DTOs;
using Store.Application.Stores.Queries.GetPublishedStorefrontBySlug;

namespace ECommerce.API.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="StorefrontController"/> class.
    /// </summary>
    [TestClass]
    public class StorefrontControllerTests
    {
        /// <summary>
        /// Tests that GetPublishedStoreFrontBySlug returns OkObjectResult with the StorefrontDto
        /// when the sender returns a valid result.
        /// </summary>
        [TestMethod]
        public async Task GetPublishedStoreFrontBySlug_ValidSlugReturnsResult_ReturnsOkWithDto()
        {
            // Arrange
            var slug = "test-store";
            var cancellationToken = CancellationToken.None;
            var expectedDto = new StorefrontDto(
                Guid.NewGuid(),
                "Test Store",
                slug,
                "Test Description",
                "https://example.com/logo.png"
            );

            var mockSender = new Mock<ISender>();
            mockSender
                .Setup(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken))
                .ReturnsAsync(expectedDto);

            var controller = new StorefrontController(mockSender.Object);

            // Act
            var result = await controller.GetPublishedStoreFrontBySlug(slug, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(expectedDto, okResult.Value);
            mockSender.Verify(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that GetPublishedStoreFrontBySlug returns NotFoundResult
        /// when the sender returns null.
        /// </summary>
        [TestMethod]
        public async Task GetPublishedStoreFrontBySlug_SlugNotFound_ReturnsNotFound()
        {
            // Arrange
            var slug = "non-existent-store";
            var cancellationToken = CancellationToken.None;

            var mockSender = new Mock<ISender>();
            mockSender
                .Setup(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken))
                .ReturnsAsync((StorefrontDto?)null);

            var controller = new StorefrontController(mockSender.Object);

            // Act
            var result = await controller.GetPublishedStoreFrontBySlug(slug, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockSender.Verify(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that GetPublishedStoreFrontBySlug handles empty string slug correctly
        /// by passing it to the sender and returning the appropriate result.
        /// </summary>
        [TestMethod]
        public async Task GetPublishedStoreFrontBySlug_EmptySlug_ReturnsNotFound()
        {
            // Arrange
            var slug = string.Empty;
            var cancellationToken = CancellationToken.None;

            var mockSender = new Mock<ISender>();
            mockSender
                .Setup(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken))
                .ReturnsAsync((StorefrontDto?)null);

            var controller = new StorefrontController(mockSender.Object);

            // Act
            var result = await controller.GetPublishedStoreFrontBySlug(slug, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockSender.Verify(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that GetPublishedStoreFrontBySlug handles whitespace-only slug correctly
        /// by passing it to the sender and returning the appropriate result.
        /// </summary>
        [TestMethod]
        public async Task GetPublishedStoreFrontBySlug_WhitespaceSlug_ReturnsNotFound()
        {
            // Arrange
            var slug = "   ";
            var cancellationToken = CancellationToken.None;

            var mockSender = new Mock<ISender>();
            mockSender
                .Setup(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken))
                .ReturnsAsync((StorefrontDto?)null);

            var controller = new StorefrontController(mockSender.Object);

            // Act
            var result = await controller.GetPublishedStoreFrontBySlug(slug, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockSender.Verify(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that GetPublishedStoreFrontBySlug handles slug with special characters correctly.
        /// </summary>
        /// <param name="slug">The slug containing special characters to test.</param>
        [TestMethod]
        [DataRow("store-with-dashes")]
        [DataRow("store_with_underscores")]
        [DataRow("store.with.dots")]
        [DataRow("store@special#chars")]
        [DataRow("store/with/slashes")]
        [DataRow("store\\with\\backslashes")]
        public async Task GetPublishedStoreFrontBySlug_SlugWithSpecialCharacters_ProcessesCorrectly(string slug)
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var expectedDto = new StorefrontDto(
                Guid.NewGuid(),
                "Test Store",
                slug,
                null,
                null
            );

            var mockSender = new Mock<ISender>();
            mockSender
                .Setup(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken))
                .ReturnsAsync(expectedDto);

            var controller = new StorefrontController(mockSender.Object);

            // Act
            var result = await controller.GetPublishedStoreFrontBySlug(slug, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(expectedDto, okResult.Value);
            mockSender.Verify(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that GetPublishedStoreFrontBySlug handles very long slug correctly.
        /// </summary>
        [TestMethod]
        public async Task GetPublishedStoreFrontBySlug_VeryLongSlug_ProcessesCorrectly()
        {
            // Arrange
            var slug = new string('a', 10000);
            var cancellationToken = CancellationToken.None;

            var mockSender = new Mock<ISender>();
            mockSender
                .Setup(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken))
                .ReturnsAsync((StorefrontDto?)null);

            var controller = new StorefrontController(mockSender.Object);

            // Act
            var result = await controller.GetPublishedStoreFrontBySlug(slug, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockSender.Verify(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that GetPublishedStoreFrontBySlug respects the cancellation token
        /// by passing it correctly to the sender.
        /// </summary>
        [TestMethod]
        public async Task GetPublishedStoreFrontBySlug_WithCancellationToken_PassesTokenToSender()
        {
            // Arrange
            var slug = "test-store";
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var expectedDto = new StorefrontDto(
                Guid.NewGuid(),
                "Test Store",
                slug,
                null,
                null
            );

            var mockSender = new Mock<ISender>();
            mockSender
                .Setup(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken))
                .ReturnsAsync(expectedDto);

            var controller = new StorefrontController(mockSender.Object);

            // Act
            var result = await controller.GetPublishedStoreFrontBySlug(slug, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSender.Verify(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
        }

        /// <summary>
        /// Tests that GetPublishedStoreFrontBySlug handles slug with unicode characters correctly.
        /// </summary>
        [TestMethod]
        public async Task GetPublishedStoreFrontBySlug_UnicodeSlug_ProcessesCorrectly()
        {
            // Arrange
            var slug = "store-名前-αβγ-مخزن";
            var cancellationToken = CancellationToken.None;
            var expectedDto = new StorefrontDto(
                Guid.NewGuid(),
                "Unicode Store",
                slug,
                null,
                null
            );

            var mockSender = new Mock<ISender>();
            mockSender
                .Setup(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken))
                .ReturnsAsync(expectedDto);

            var controller = new StorefrontController(mockSender.Object);

            // Act
            var result = await controller.GetPublishedStoreFrontBySlug(slug, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(expectedDto, okResult.Value);
        }

        /// <summary>
        /// Tests that GetPublishedStoreFrontBySlug handles control characters in slug correctly.
        /// </summary>
        [TestMethod]
        public async Task GetPublishedStoreFrontBySlug_SlugWithControlCharacters_ProcessesCorrectly()
        {
            // Arrange
            var slug = "store\t\n\r";
            var cancellationToken = CancellationToken.None;

            var mockSender = new Mock<ISender>();
            mockSender
                .Setup(s => s.Send(It.Is<GetPublishedStoreFrontBySlugQuery>(q => q.Slug == slug), cancellationToken))
                .ReturnsAsync((StorefrontDto?)null);

            var controller = new StorefrontController(mockSender.Object);

            // Act
            var result = await controller.GetPublishedStoreFrontBySlug(slug, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests that GetPublishedStoreFrontBySlug creates the correct query object
        /// with the provided slug parameter.
        /// </summary>
        [TestMethod]
        public async Task GetPublishedStoreFrontBySlug_CreatesCorrectQuery_WithProvidedSlug()
        {
            // Arrange
            var slug = "specific-store-slug";
            var cancellationToken = CancellationToken.None;
            GetPublishedStoreFrontBySlugQuery? capturedQuery = null;

            var mockSender = new Mock<ISender>();
            mockSender
                .Setup(s => s.Send(It.IsAny<GetPublishedStoreFrontBySlugQuery>(), cancellationToken))
                .Callback<IRequest<StorefrontDto?>, CancellationToken>((query, token) =>
                {
                    capturedQuery = query as GetPublishedStoreFrontBySlugQuery;
                })
                .ReturnsAsync((StorefrontDto?)null);

            var controller = new StorefrontController(mockSender.Object);

            // Act
            await controller.GetPublishedStoreFrontBySlug(slug, cancellationToken);

            // Assert
            Assert.IsNotNull(capturedQuery);
            Assert.AreEqual(slug, capturedQuery.Slug);
        }

        /// <summary>
        /// Tests that the constructor successfully initializes with a valid ISender instance.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidSender_InitializesSuccessfully()
        {
            // Arrange
            var mockSender = new Mock<ISender>();

            // Act
            var controller = new StorefrontController(mockSender.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that the constructor accepts a null sender parameter without throwing an exception.
        /// This verifies the actual behavior where no null guard is present.
        /// </summary>
        [TestMethod]
        public void Constructor_NullSender_DoesNotThrow()
        {
            // Arrange
            ISender? nullSender = null;

            // Act
            var controller = new StorefrontController(nullSender!);

            // Assert
            Assert.IsNotNull(controller);
        }
    }
}
