using System;

using Store.Application;
using Store.Application.Exceptions;

namespace Store.Application.Exceptions.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="StoreNotFoundException"/> class.
    /// </summary>
    [TestClass]
    public sealed class StoreNotFoundExceptionTests
    {
        /// <summary>
        /// Tests that the constructor with tenantId and identifier parameters correctly initializes
        /// the exception with the expected TenantId property and formatted message.
        /// </summary>
        /// <param name="tenantIdString">The string representation of the Guid to test.</param>
        /// <param name="identifier">The identifier parameter to test.</param>
        [TestMethod]
        [DataRow("00000000-0000-0000-0000-000000000000", "TenantId")]
        [DataRow("12345678-1234-1234-1234-123456789abc", "TenantId")]
        [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff", "StoreId")]
        [DataRow("550e8400-e29b-41d4-a716-446655440000", "ID")]
        public void Constructor_WithValidTenantIdAndIdentifier_SetsTenantIdAndFormatsMessage(string tenantIdString, string identifier)
        {
            // Arrange
            Guid tenantId = Guid.Parse(tenantIdString);
            string expectedMessage = $"Store with {identifier} {tenantId} not found.";

            // Act
            StoreNotFoundException exception = new StoreNotFoundException(tenantId, identifier);

            // Assert
            Assert.AreEqual(tenantId, exception.TenantId);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor with a null identifier parameter does not throw an exception
        /// and produces a message with "null" in place of the identifier.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullIdentifier_SetsTenantIdAndFormatsMessageWithNull()
        {
            // Arrange
            Guid tenantId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
            string? identifier = null;
            string expectedMessage = $"Store with {identifier} {tenantId} not found.";

            // Act
            StoreNotFoundException exception = new StoreNotFoundException(tenantId, identifier!);

            // Assert
            Assert.AreEqual(tenantId, exception.TenantId);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor with an empty string identifier correctly formats the message
        /// with an empty string in place of the identifier.
        /// </summary>
        [TestMethod]
        public void Constructor_WithEmptyStringIdentifier_SetsTenantIdAndFormatsMessage()
        {
            // Arrange
            Guid tenantId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
            string identifier = string.Empty;
            string expectedMessage = $"Store with {identifier} {tenantId} not found.";

            // Act
            StoreNotFoundException exception = new StoreNotFoundException(tenantId, identifier);

            // Assert
            Assert.AreEqual(tenantId, exception.TenantId);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor with a whitespace-only identifier correctly formats the message
        /// with the whitespace preserved.
        /// </summary>
        [TestMethod]
        public void Constructor_WithWhitespaceIdentifier_SetsTenantIdAndFormatsMessage()
        {
            // Arrange
            Guid tenantId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
            string identifier = "   ";
            string expectedMessage = $"Store with {identifier} {tenantId} not found.";

            // Act
            StoreNotFoundException exception = new StoreNotFoundException(tenantId, identifier);

            // Assert
            Assert.AreEqual(tenantId, exception.TenantId);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor with an identifier containing special characters correctly
        /// formats the message with the special characters preserved.
        /// </summary>
        [TestMethod]
        [DataRow("Tenant@Id#123")]
        [DataRow("Store-ID_01")]
        [DataRow("ID\nwith\nnewlines")]
        [DataRow("ID\twith\ttabs")]
        [DataRow("ID with spaces")]
        public void Constructor_WithSpecialCharactersInIdentifier_SetsTenantIdAndFormatsMessage(string identifier)
        {
            // Arrange
            Guid tenantId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
            string expectedMessage = $"Store with {identifier} {tenantId} not found.";

            // Act
            StoreNotFoundException exception = new StoreNotFoundException(tenantId, identifier);

            // Assert
            Assert.AreEqual(tenantId, exception.TenantId);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor with a very long identifier correctly formats the message
        /// without truncation or errors.
        /// </summary>
        [TestMethod]
        public void Constructor_WithVeryLongIdentifier_SetsTenantIdAndFormatsMessage()
        {
            // Arrange
            Guid tenantId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
            string identifier = new string('A', 10000);
            string expectedMessage = $"Store with {identifier} {tenantId} not found.";

            // Act
            StoreNotFoundException exception = new StoreNotFoundException(tenantId, identifier);

            // Assert
            Assert.AreEqual(tenantId, exception.TenantId);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor with Guid.Empty correctly sets the TenantId property
        /// and formats the message.
        /// </summary>
        [TestMethod]
        public void Constructor_WithEmptyGuid_SetsTenantIdAndFormatsMessage()
        {
            // Arrange
            Guid tenantId = Guid.Empty;
            string identifier = "TenantId";
            string expectedMessage = $"Store with {identifier} {tenantId} not found.";

            // Act
            StoreNotFoundException exception = new StoreNotFoundException(tenantId, identifier);

            // Assert
            Assert.AreEqual(tenantId, exception.TenantId);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor with tenantId parameter correctly sets the TenantId property
        /// for various GUID values including empty GUID.
        /// </summary>
        /// <param name="tenantIdString">The string representation of the GUID to test.</param>
        [TestMethod]
        [DataRow("00000000-0000-0000-0000-000000000000")]
        [DataRow("12345678-1234-1234-1234-123456789012")]
        [DataRow("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
        [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff")]
        public void Constructor_WithTenantId_SetsTenantIdProperty(string tenantIdString)
        {
            // Arrange
            Guid tenantId = Guid.Parse(tenantIdString);

            // Act
            StoreNotFoundException exception = new StoreNotFoundException(tenantId);

            // Assert
            Assert.AreEqual(tenantId, exception.TenantId);
        }

        /// <summary>
        /// Tests that the constructor with tenantId parameter creates the correct exception message
        /// for various GUID values.
        /// </summary>
        /// <param name="tenantIdString">The string representation of the GUID to test.</param>
        [TestMethod]
        [DataRow("00000000-0000-0000-0000-000000000000")]
        [DataRow("12345678-1234-1234-1234-123456789012")]
        [DataRow("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
        public void Constructor_WithTenantId_SetsCorrectMessage(string tenantIdString)
        {
            // Arrange
            Guid tenantId = Guid.Parse(tenantIdString);
            string expectedMessage = $"Store with Tenant ID {tenantId} not found.";

            // Act
            StoreNotFoundException exception = new StoreNotFoundException(tenantId);

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the exception with Guid.Empty creates a valid exception with correct property values.
        /// </summary>
        [TestMethod]
        public void Constructor_WithEmptyGuid_CreatesValidException()
        {
            // Arrange
            Guid tenantId = Guid.Empty;
            string expectedMessage = $"Store with Tenant ID {Guid.Empty} not found.";

            // Act
            StoreNotFoundException exception = new StoreNotFoundException(tenantId);

            // Assert
            Assert.AreEqual(tenantId, exception.TenantId);
            Assert.AreEqual(expectedMessage, exception.Message);
            Assert.IsNotNull(exception);
        }

        /// <summary>
        /// Tests that the exception inherits from ApplicationException correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_WithTenantId_InheritsFromApplicationException()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();

            // Act
            StoreNotFoundException exception = new StoreNotFoundException(tenantId);

            // Assert
            Assert.IsInstanceOfType(exception, typeof(ApplicationException));
        }
    }
}