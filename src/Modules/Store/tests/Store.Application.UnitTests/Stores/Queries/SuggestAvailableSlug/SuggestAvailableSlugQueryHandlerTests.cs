using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application;
using Store.Application.DTOs;
using Store.Application.Stores.Queries;
using Store.Application.Stores.Queries.SuggestAvailableSlug;
using Store.Domain;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace Store.Application.Stores.Queries.SuggestAvailableSlug.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="SuggestAvailableSlugQueryHandler"/>.
    /// </summary>
    [TestClass]
    public sealed class SuggestAvailableSlugQueryHandlerTests
    {
        /// <summary>
        /// Tests that the constructor successfully creates an instance when provided with a valid store repository.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidRepository_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockRepository = new Mock<IStoreRepository>();

            // Act
            var handler = new SuggestAvailableSlugQueryHandler(mockRepository.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Tests that the constructor does not throw an exception when provided with a null repository.
        /// Note: The constructor does not perform null validation, so this documents the current behavior.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullRepository_DoesNotThrowException()
        {
            // Arrange
            IStoreRepository? nullRepository = null;

            // Act
            var handler = new SuggestAvailableSlugQueryHandler(nullRepository!);

            // Assert
            Assert.IsNotNull(handler);
        }
    }
}