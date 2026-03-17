using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Application.Exceptions;
using System;


namespace Store.Application.Exceptions.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="DuplicateStoreSlugException"/> class.
    /// </summary>
    [TestClass]
    public sealed class DuplicateStoreSlugExceptionTests
    {
        /// <summary>
        /// Tests that the constructor properly initializes the exception with various slug values,
        /// setting the Slug property and formatting the Message correctly.
        /// </summary>
        /// <param name="slug">The slug value to test.</param>
        /// <param name="expectedMessage">The expected exception message.</param>
        [TestMethod]
        [DataRow("my-store-slug", "A store with slug 'my-store-slug' already exists.")]
        [DataRow("", "A store with slug '' already exists.")]
        [DataRow("   ", "A store with slug '   ' already exists.")]
        [DataRow("store-with-!@#$%^&*()", "A store with slug 'store-with-!@#$%^&*()' already exists.")]
        [DataRow("store's-name", "A store with slug 'store's-name' already exists.")]
        [DataRow("UPPERCASE-SLUG", "A store with slug 'UPPERCASE-SLUG' already exists.")]
        [DataRow("slug-with-numbers-123", "A store with slug 'slug-with-numbers-123' already exists.")]
        public void Constructor_ValidSlugValue_SetsSlugPropertyAndMessage(string slug, string expectedMessage)
        {
            // Arrange & Act
            DuplicateStoreSlugException exception = new DuplicateStoreSlugException(slug);

            // Assert
            Assert.AreEqual(slug, exception.Slug);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor properly handles a very long slug value,
        /// correctly setting the Slug property and including the long slug in the message.
        /// </summary>
        [TestMethod]
        public void Constructor_VeryLongSlug_SetsSlugPropertyAndMessage()
        {
            // Arrange
            string veryLongSlug = new string('a', 10000);
            string expectedMessage = $"A store with slug '{veryLongSlug}' already exists.";

            // Act
            DuplicateStoreSlugException exception = new DuplicateStoreSlugException(veryLongSlug);

            // Assert
            Assert.AreEqual(veryLongSlug, exception.Slug);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor handles null slug value by setting the Slug property to null
        /// and including an empty representation in the message.
        /// This tests runtime behavior when null is passed despite non-nullable parameter annotation.
        /// </summary>
        [TestMethod]
        public void Constructor_NullSlug_SetsSlugPropertyToNull()
        {
            // Arrange
            string? slug = null;
            string expectedMessage = "A store with slug '' already exists.";

            // Act
            DuplicateStoreSlugException exception = new DuplicateStoreSlugException(slug!);

            // Assert
            Assert.IsNull(exception.Slug);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the exception can be thrown and caught as DuplicateStoreSlugException,
        /// verifying it functions properly as an exception type.
        /// </summary>
        [TestMethod]
        public void Constructor_ThrowException_CanBeCaughtAsDuplicateStoreSlugException()
        {
            // Arrange
            string slug = "test-slug";
            DuplicateStoreSlugException? caughtException = null;

            // Act
            try
            {
                throw new DuplicateStoreSlugException(slug);
            }
            catch (DuplicateStoreSlugException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.IsNotNull(caughtException);
            Assert.AreEqual(slug, caughtException.Slug);
            Assert.AreEqual($"A store with slug '{slug}' already exists.", caughtException.Message);
        }

        /// <summary>
        /// Tests that the exception can be caught as ApplicationException,
        /// verifying the inheritance hierarchy works correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_ThrowException_CanBeCaughtAsApplicationException()
        {
            // Arrange
            string slug = "test-slug";
            ApplicationException? caughtException = null;

            // Act
            try
            {
                throw new DuplicateStoreSlugException(slug);
            }
            catch (ApplicationException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.IsNotNull(caughtException);
            Assert.IsInstanceOfType(caughtException, typeof(DuplicateStoreSlugException));
        }
    }
}