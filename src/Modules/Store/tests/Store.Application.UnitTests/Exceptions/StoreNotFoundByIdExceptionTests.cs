using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Application;
using Store.Application.Exceptions;
using System;


namespace Store.Application.Exceptions.UnitTests
{
    /// <summary>
    /// Tests for the <see cref="StoreNotFoundByIdException"/> class.
    /// </summary>
    [TestClass]
    public sealed class StoreNotFoundByIdExceptionTests
    {
        /// <summary>
        /// Tests that the constructor correctly initializes the StoreId property with the provided Guid value.
        /// </summary>
        /// <param name="storeId">The store ID to test.</param>
        [TestMethod]
        [DataRow("00000000-0000-0000-0000-000000000000", DisplayName = "Constructor_EmptyGuid_SetsStoreIdProperty")]
        [DataRow("12345678-1234-1234-1234-123456789012", DisplayName = "Constructor_ValidGuid_SetsStoreIdProperty")]
        [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff", DisplayName = "Constructor_MaxGuid_SetsStoreIdProperty")]
        [DataRow("a1b2c3d4-e5f6-7890-abcd-ef1234567890", DisplayName = "Constructor_AnotherValidGuid_SetsStoreIdProperty")]
        public void Constructor_WithGuid_SetsStoreIdProperty(string guidString)
        {
            // Arrange
            Guid storeId = Guid.Parse(guidString);

            // Act
            StoreNotFoundByIdException exception = new StoreNotFoundByIdException(storeId);

            // Assert
            Assert.AreEqual(storeId, exception.StoreId);
        }

        /// <summary>
        /// Tests that the constructor correctly formats the exception message with the provided store ID.
        /// </summary>
        /// <param name="guidString">The store ID as a string.</param>
        /// <param name="expectedMessage">The expected exception message.</param>
        [TestMethod]
        [DataRow("00000000-0000-0000-0000-000000000000", "Store with ID 00000000-0000-0000-0000-000000000000 not found.", DisplayName = "Constructor_EmptyGuid_SetsCorrectMessage")]
        [DataRow("12345678-1234-1234-1234-123456789012", "Store with ID 12345678-1234-1234-1234-123456789012 not found.", DisplayName = "Constructor_ValidGuid_SetsCorrectMessage")]
        [DataRow("a1b2c3d4-e5f6-7890-abcd-ef1234567890", "Store with ID a1b2c3d4-e5f6-7890-abcd-ef1234567890 not found.", DisplayName = "Constructor_AnotherValidGuid_SetsCorrectMessage")]
        public void Constructor_WithGuid_SetsCorrectExceptionMessage(string guidString, string expectedMessage)
        {
            // Arrange
            Guid storeId = Guid.Parse(guidString);

            // Act
            StoreNotFoundByIdException exception = new StoreNotFoundByIdException(storeId);

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the exception inherits from ApplicationException.
        /// </summary>
        [TestMethod]
        public void Constructor_CreatesException_InheritsFromApplicationException()
        {
            // Arrange
            Guid storeId = Guid.NewGuid();

            // Act
            StoreNotFoundByIdException exception = new StoreNotFoundByIdException(storeId);

            // Assert
            Assert.IsInstanceOfType(exception, typeof(ApplicationException));
        }
    }
}