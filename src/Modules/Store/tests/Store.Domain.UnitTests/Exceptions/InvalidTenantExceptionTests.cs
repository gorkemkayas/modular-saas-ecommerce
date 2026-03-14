using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Domain.Exceptions;


namespace Store.Domain.Exceptions.UnitTests
{
    /// <summary>
    /// Unit tests for the InvalidTenantException class.
    /// </summary>
    [TestClass]
    public sealed class InvalidTenantExceptionTests
    {
        /// <summary>
        /// Tests that the parameterless constructor creates a valid InvalidTenantException instance.
        /// Verifies that the exception object is not null and is of the correct type.
        /// </summary>
        [TestMethod]
        public void Constructor_WhenCalled_CreatesValidInstance()
        {
            // Act
            var exception = new InvalidTenantException();

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(InvalidTenantException));
        }

        /// <summary>
        /// Tests that the parameterless constructor sets the correct error message.
        /// Verifies that the Message property contains "TenantId cannot be empty."
        /// </summary>
        [TestMethod]
        public void Constructor_WhenCalled_SetsCorrectMessage()
        {
            // Act
            var exception = new InvalidTenantException();

            // Assert
            Assert.AreEqual("TenantId cannot be empty.", exception.Message);
        }

        /// <summary>
        /// Tests that InvalidTenantException inherits from DomainException.
        /// Verifies the inheritance chain is properly established.
        /// </summary>
        [TestMethod]
        public void Constructor_WhenCalled_InheritsFromDomainException()
        {
            // Act
            var exception = new InvalidTenantException();

            // Assert
            Assert.IsInstanceOfType(exception, typeof(DomainException));
        }

        /// <summary>
        /// Tests that InvalidTenantException can be thrown and caught as InvalidTenantException.
        /// Verifies proper exception throwing and catching behavior.
        /// </summary>
        [TestMethod]
        public void Constructor_WhenThrown_CanBeCaughtAsInvalidTenantException()
        {
            // Arrange
            InvalidTenantException? caughtException = null;

            // Act
            try
            {
                throw new InvalidTenantException();
            }
            catch (InvalidTenantException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.IsNotNull(caughtException);
            Assert.AreEqual("TenantId cannot be empty.", caughtException.Message);
        }

        /// <summary>
        /// Tests that InvalidTenantException can be thrown and caught as DomainException.
        /// Verifies that the exception can be caught using its base type.
        /// </summary>
        [TestMethod]
        public void Constructor_WhenThrown_CanBeCaughtAsDomainException()
        {
            // Arrange
            DomainException? caughtException = null;

            // Act
            try
            {
                throw new InvalidTenantException();
            }
            catch (DomainException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.IsNotNull(caughtException);
            Assert.IsInstanceOfType(caughtException, typeof(InvalidTenantException));
            Assert.AreEqual("TenantId cannot be empty.", caughtException.Message);
        }
    }
}