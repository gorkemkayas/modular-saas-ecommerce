using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Application;
using Store.Application.Exceptions;

namespace Store.Application.Exceptions.UnitTests
{
    [TestClass]
    public sealed class UnauthorizedStoreAccessExceptionTests
    {
        /// <summary>
        /// Tests that the constructor with tenantId and storeId parameters correctly sets the TenantId property.
        /// </summary>
        /// <param name="tenantIdString">The tenant ID as a string.</param>
        /// <param name="storeIdString">The store ID as a string.</param>
        [TestMethod]
        [DataRow("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
        [DataRow("550e8400-e29b-41d4-a716-446655440000", "00000000-0000-0000-0000-000000000000")]
        [DataRow("00000000-0000-0000-0000-000000000000", "7c9e6679-7425-40de-944b-e07fc1f90ae7")]
        [DataRow("550e8400-e29b-41d4-a716-446655440000", "7c9e6679-7425-40de-944b-e07fc1f90ae7")]
        public void Constructor_WithTenantIdAndStoreId_SetsTenantIdProperty(string tenantIdString, string storeIdString)
        {
            // Arrange
            Guid tenantId = Guid.Parse(tenantIdString);
            Guid storeId = Guid.Parse(storeIdString);

            // Act
            UnauthorizedStoreAccessException exception = new UnauthorizedStoreAccessException(tenantId, storeId);

            // Assert
            Assert.AreEqual(tenantId, exception.TenantId);
        }

        /// <summary>
        /// Tests that the constructor with tenantId and storeId parameters correctly sets the StoreId property.
        /// </summary>
        /// <param name="tenantIdString">The tenant ID as a string.</param>
        /// <param name="storeIdString">The store ID as a string.</param>
        [TestMethod]
        [DataRow("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
        [DataRow("550e8400-e29b-41d4-a716-446655440000", "00000000-0000-0000-0000-000000000000")]
        [DataRow("00000000-0000-0000-0000-000000000000", "7c9e6679-7425-40de-944b-e07fc1f90ae7")]
        [DataRow("550e8400-e29b-41d4-a716-446655440000", "7c9e6679-7425-40de-944b-e07fc1f90ae7")]
        public void Constructor_WithTenantIdAndStoreId_SetsStoreIdProperty(string tenantIdString, string storeIdString)
        {
            // Arrange
            Guid tenantId = Guid.Parse(tenantIdString);
            Guid storeId = Guid.Parse(storeIdString);

            // Act
            UnauthorizedStoreAccessException exception = new UnauthorizedStoreAccessException(tenantId, storeId);

            // Assert
            Assert.AreEqual(storeId, exception.StoreId);
        }

        /// <summary>
        /// Tests that the constructor with tenantId and storeId parameters creates a message containing both IDs.
        /// </summary>
        /// <param name="tenantIdString">The tenant ID as a string.</param>
        /// <param name="storeIdString">The store ID as a string.</param>
        [TestMethod]
        [DataRow("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
        [DataRow("550e8400-e29b-41d4-a716-446655440000", "7c9e6679-7425-40de-944b-e07fc1f90ae7")]
        public void Constructor_WithTenantIdAndStoreId_CreatesMessageContainingBothIds(string tenantIdString, string storeIdString)
        {
            // Arrange
            Guid tenantId = Guid.Parse(tenantIdString);
            Guid storeId = Guid.Parse(storeIdString);

            // Act
            UnauthorizedStoreAccessException exception = new UnauthorizedStoreAccessException(tenantId, storeId);

            // Assert
            string expectedMessage = $"Unauthorized access to store {storeId} for Tenant ID {tenantId}.";
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the exception inherits from ApplicationException.
        /// </summary>
        [TestMethod]
        public void Constructor_WithTenantIdAndStoreId_InheritsFromApplicationException()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            Guid storeId = Guid.NewGuid();

            // Act
            UnauthorizedStoreAccessException exception = new UnauthorizedStoreAccessException(tenantId, storeId);

            // Assert
            Assert.IsInstanceOfType(exception, typeof(ApplicationException));
        }

        /// <summary>
        /// Tests that the constructor with tenantId parameter correctly initializes the exception
        /// with the provided tenant ID, sets TenantId property, leaves StoreId as null, and formats
        /// the message correctly.
        /// </summary>
        /// <param name="tenantId">The tenant ID to use for the exception.</param>
        /// <param name="expectedMessage">The expected formatted message.</param>
        [TestMethod]
        [DataRow("00000000-0000-0000-0000-000000000000", "Unauthorized access to store for Tenant ID 00000000-0000-0000-0000-000000000000.")]
        [DataRow("12345678-1234-1234-1234-123456789abc", "Unauthorized access to store for Tenant ID 12345678-1234-1234-1234-123456789abc.")]
        [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff", "Unauthorized access to store for Tenant ID ffffffff-ffff-ffff-ffff-ffffffffffff.")]
        public void Constructor_WithTenantId_SetsPropertiesAndMessageCorrectly(string tenantId, string expectedMessage)
        {
            // Arrange
            Guid tenantIdGuid = Guid.Parse(tenantId);

            // Act
            UnauthorizedStoreAccessException exception = new UnauthorizedStoreAccessException(tenantIdGuid);

            // Assert
            Assert.AreEqual(tenantIdGuid, exception.TenantId);
            Assert.IsNull(exception.StoreId);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor with tenantId parameter correctly handles Guid.Empty value
        /// and creates a valid exception instance with properly formatted message.
        /// </summary>
        [TestMethod]
        public void Constructor_WithEmptyGuid_CreatesValidException()
        {
            // Arrange
            Guid tenantId = Guid.Empty;

            // Act
            UnauthorizedStoreAccessException exception = new UnauthorizedStoreAccessException(tenantId);

            // Assert
            Assert.AreEqual(Guid.Empty, exception.TenantId);
            Assert.IsNull(exception.StoreId);
            Assert.IsTrue(exception.Message.Contains("00000000-0000-0000-0000-000000000000"));
        }

        /// <summary>
        /// Tests that the constructor with tenantId parameter correctly handles a newly generated Guid
        /// and creates a valid exception instance with the correct tenant ID.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNewGuid_CreatesValidException()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();

            // Act
            UnauthorizedStoreAccessException exception = new UnauthorizedStoreAccessException(tenantId);

            // Assert
            Assert.AreEqual(tenantId, exception.TenantId);
            Assert.IsNull(exception.StoreId);
            Assert.IsTrue(exception.Message.Contains(tenantId.ToString()));
            Assert.IsTrue(exception.Message.StartsWith("Unauthorized access to store for Tenant ID"));
        }
    }
}