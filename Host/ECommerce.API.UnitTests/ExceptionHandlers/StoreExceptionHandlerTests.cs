using System;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using ECommerce.API.ExceptionHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application.Exceptions;
using Store.Domain.Exceptions;

namespace ECommerce.API.ExceptionHandlers.UnitTests
{
    /// <summary>
    /// Unit tests for the StoreExceptionHandler class.
    /// </summary>
    [TestClass]
    public sealed class StoreExceptionHandlerTests
    {
        /// <summary>
        /// Tests that TryHandleAsync returns false when the exception is not a Store exception.
        /// Input: Non-Store exception (ArgumentException).
        /// Expected: Returns false without processing.
        /// </summary>
        [TestMethod]
        public async Task TryHandleAsync_NonStoreException_ReturnsFalse()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<StoreExceptionHandler>>();
            var mockEnvironment = new Mock<IHostEnvironment>();
            var handler = new StoreExceptionHandler(mockLogger.Object, mockEnvironment.Object);

            var mockHttpContext = new Mock<HttpContext>();
            var exception = new ArgumentException("Test exception");
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.TryHandleAsync(mockHttpContext.Object, exception, cancellationToken);

            // Assert
            Assert.IsFalse(result);
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        /// <summary>
        /// Tests that TryHandleAsync handles generic DomainException.
        /// Input: Generic DomainException (not a specific subtype).
        /// Expected: Maps to 400 status code with "Domain Rule Violation" title.
        /// </summary>
        [TestMethod]
        public async Task TryHandleAsync_GenericDomainException_HandlesCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<StoreExceptionHandler>>();
            var mockEnvironment = new Mock<IHostEnvironment>();
            mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

            var handler = new StoreExceptionHandler(mockLogger.Object, mockEnvironment.Object);

            // Mock RequestServices with proper service provider setup
            var jsonOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions();
            var mockOptions = new Mock<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
            mockOptions.Setup(x => x.Value).Returns(jsonOptions);
            
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(typeof(Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>)))
                .Returns(mockOptions.Object);

            var responseStream = new MemoryStream();
            var pipeWriter = System.IO.Pipelines.PipeWriter.Create(responseStream);
            var mockHttpContext = CreateMockHttpContext("/test/path", "trace123");
            mockHttpContext.Setup(x => x.RequestServices).Returns(mockServiceProvider.Object);
            mockHttpContext.Setup(x => x.Response.Body).Returns(responseStream);
            mockHttpContext.Setup(x => x.Response.BodyWriter).Returns(pipeWriter);
            
            // Fix: Set up the circular reference between HttpResponse and HttpContext
            mockHttpContext.Setup(x => x.Response.HttpContext).Returns(mockHttpContext.Object);
            
            var exception = new TestDomainException("Generic domain error");
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.TryHandleAsync(mockHttpContext.Object, exception, cancellationToken);

            // Assert
            Assert.IsTrue(result);
            mockHttpContext.VerifySet(x => x.Response.StatusCode = 400, Times.Once);
            await pipeWriter.FlushAsync();
            responseStream.Position = 0;
            var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual("Domain Rule Violation", problemDetails.Title);
        }

        private static Mock<HttpContext> CreateMockHttpContext(string path, string traceIdentifier, ClaimsPrincipal? user = null)
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            var mockResponse = new Mock<HttpResponse>();

            mockRequest.Setup(x => x.Path).Returns(path);
            mockHttpContext.Setup(x => x.Request).Returns(mockRequest.Object);
            mockHttpContext.Setup(x => x.TraceIdentifier).Returns(traceIdentifier);
            mockHttpContext.Setup(x => x.Response).Returns(mockResponse.Object);
            mockHttpContext.Setup(x => x.User).Returns(user ?? new ClaimsPrincipal());

            var responseStream = new MemoryStream();
            mockResponse.Setup(x => x.Body).Returns(responseStream);
            mockResponse.SetupProperty(x => x.StatusCode);
            mockResponse.SetupProperty(x => x.ContentType);

            return mockHttpContext;
        }

        /// <summary>
        /// Test implementation of DomainException for testing generic exception handling.
        /// </summary>
        private sealed class TestDomainException : DomainException
        {
            public TestDomainException(string message) : base(message) { }
        }

        /// <summary>
        /// Test implementation of ApplicationException for testing generic exception handling.
        /// </summary>
        private sealed class TestApplicationException : Store.Application.Exceptions.ApplicationException
        {
            public TestApplicationException(string message) : base(message) { }
        }

        /// <summary>
        /// Tests that the constructor successfully initializes the StoreExceptionHandler 
        /// with valid logger and environment dependencies.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_InitializesSuccessfully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<StoreExceptionHandler>>();
            var mockEnvironment = new Mock<IHostEnvironment>();

            // Act
            var handler = new StoreExceptionHandler(mockLogger.Object, mockEnvironment.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor accepts a null logger parameter.
        /// Since there is no explicit null validation in the constructor, 
        /// this documents the actual behavior when null is passed.
        /// </summary>
        [TestMethod]
        public void Constructor_NullLogger_DoesNotThrow()
        {
            // Arrange
            var mockEnvironment = new Mock<IHostEnvironment>();

            // Act
            var handler = new StoreExceptionHandler(null!, mockEnvironment.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor accepts a null environment parameter.
        /// Since there is no explicit null validation in the constructor, 
        /// this documents the actual behavior when null is passed.
        /// </summary>
        [TestMethod]
        public void Constructor_NullEnvironment_DoesNotThrow()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<StoreExceptionHandler>>();

            // Act
            var handler = new StoreExceptionHandler(mockLogger.Object, null!);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor accepts null for both parameters.
        /// Since there is no explicit null validation in the constructor, 
        /// this documents the actual behavior when both parameters are null.
        /// </summary>
        [TestMethod]
        public void Constructor_BothParametersNull_DoesNotThrow()
        {
            // Arrange & Act
            var handler = new StoreExceptionHandler(null!, null!);

            // Assert
            Assert.IsNotNull(handler);
        }
    }
}