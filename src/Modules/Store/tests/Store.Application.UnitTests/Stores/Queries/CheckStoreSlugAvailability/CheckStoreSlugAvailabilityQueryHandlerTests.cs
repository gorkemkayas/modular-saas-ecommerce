using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application.Stores.Queries.CheckStoreSlugAvailability;
using Store.Domain.Stores;
using System;


namespace Store.Application.Stores.Queries.CheckStoreSlugAvailability.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="CheckStoreSlugAvailabilityQueryHandler"/> class.
    /// </summary>
    [TestClass]
    public sealed class CheckStoreSlugAvailabilityQueryHandlerTests
    {
        /// <summary>
        /// Tests that the constructor successfully creates an instance when provided with a valid store repository.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidStoreRepository_CreatesInstance()
        {
            // Arrange
            var mockStoreRepository = new Mock<IStoreRepository>();

            // Act
            var handler = new CheckStoreSlugAvailabilityQueryHandler(mockStoreRepository.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor accepts a null store repository without throwing an exception.
        /// This documents that the current implementation lacks null validation, which violates the non-nullable parameter contract.
        /// </summary>
        [TestMethod]
        public void Constructor_NullStoreRepository_DoesNotThrowException()
        {
            // Arrange
            IStoreRepository? nullRepository = null;

            // Act
            var handler = new CheckStoreSlugAvailabilityQueryHandler(nullRepository!);

            // Assert
            Assert.IsNotNull(handler);
        }
    }
}