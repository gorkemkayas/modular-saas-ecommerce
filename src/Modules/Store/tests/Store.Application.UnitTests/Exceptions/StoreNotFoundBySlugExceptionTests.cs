using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Application.Exceptions;
using System;


namespace Store.Application.Exceptions.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="StoreNotFoundBySlugException"/> class.
    /// </summary>
    [TestClass]
    public sealed class StoreNotFoundBySlugExceptionTests
    {
        /// <summary>
        /// Tests that the constructor properly initializes the exception with various valid slug values,
        /// setting the Slug property and formatting the Message correctly.
        /// </summary>
        /// <param name="slug">The slug value to test.</param>
        [TestMethod]
        [DataRow("my-store")]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("store-with-very-long-name-that-exceeds-normal-expectations-and-continues-for-a-while-to-test-boundary-conditions")]
        [DataRow("store!@#$%^&*()_+-=[]{}|;:',.<>?/~`")]
        [DataRow("store\twith\ttabs")]
        [DataRow("store\nwith\nnewlines")]
        [DataRow("مخزن")]
        [DataRow("商店")]
        public void Constructor_ValidSlugValues_SetsSlugPropertyAndFormatsMessage(string slug)
        {
            // Arrange
            var expectedMessage = $"Store with slug '{slug}' not found.";

            // Act
            var exception = new StoreNotFoundBySlugException(slug);

            // Assert
            Assert.AreEqual(slug, exception.Slug);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructed exception is an instance of ApplicationException,
        /// verifying proper inheritance chain.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidSlug_CreatesInstanceOfApplicationException()
        {
            // Arrange
            var slug = "test-store";

            // Act
            var exception = new StoreNotFoundBySlugException(slug);

            // Assert
            Assert.IsInstanceOfType(exception, typeof(ApplicationException));
        }

        /// <summary>
        /// Tests that the constructed exception is an instance of Exception,
        /// verifying it's part of the standard exception hierarchy.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidSlug_CreatesInstanceOfException()
        {
            // Arrange
            var slug = "test-store";

            // Act
            var exception = new StoreNotFoundBySlugException(slug);

            // Assert
            Assert.IsInstanceOfType(exception, typeof(Exception));
        }

        /// <summary>
        /// Tests that the Slug property returns the exact value passed to the constructor,
        /// ensuring no transformation or modification occurs.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidSlug_SlugPropertyReturnsExactValue()
        {
            // Arrange
            var slug = "  untrimmed-slug  ";

            // Act
            var exception = new StoreNotFoundBySlugException(slug);

            // Assert
            Assert.AreSame(slug, exception.Slug);
        }

        /// <summary>
        /// Tests that the exception message format is correct and includes the slug value
        /// with proper formatting when slug contains single quotes.
        /// </summary>
        [TestMethod]
        public void Constructor_SlugWithSingleQuotes_MessageFormattedCorrectly()
        {
            // Arrange
            var slug = "store'with'quotes";
            var expectedMessage = $"Store with slug '{slug}' not found.";

            // Act
            var exception = new StoreNotFoundBySlugException(slug);

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }
    }
}