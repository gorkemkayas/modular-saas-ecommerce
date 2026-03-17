using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Domain;
using Store.Domain.Exceptions;


namespace Store.Domain.Exceptions.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="InvalidStoreNameException"/> class.
    /// </summary>
    [TestClass]
    public sealed class InvalidStoreNameExceptionTests
    {
        /// <summary>
        /// Verifies that the parameterless constructor creates a valid instance
        /// with the expected error message "Store name cannot be empty.".
        /// </summary>
        [TestMethod]
        public void Constructor_NoParameters_CreatesExceptionWithCorrectMessage()
        {
            // Arrange & Act
            var exception = new InvalidStoreNameException();

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual("Store name cannot be empty.", exception.Message);
        }

        /// <summary>
        /// Verifies that the created exception is an instance of InvalidStoreNameException.
        /// </summary>
        [TestMethod]
        public void Constructor_NoParameters_CreatesCorrectExceptionType()
        {
            // Arrange & Act
            var exception = new InvalidStoreNameException();

            // Assert
            Assert.IsInstanceOfType(exception, typeof(InvalidStoreNameException));
        }

        /// <summary>
        /// Verifies that the created exception inherits from DomainException.
        /// </summary>
        [TestMethod]
        public void Constructor_NoParameters_InheritsFromDomainException()
        {
            // Arrange & Act
            var exception = new InvalidStoreNameException();

            // Assert
            Assert.IsInstanceOfType(exception, typeof(DomainException));
        }

        /// <summary>
        /// Verifies that the created exception inherits from the base Exception class.
        /// </summary>
        [TestMethod]
        public void Constructor_NoParameters_InheritsFromException()
        {
            // Arrange & Act
            var exception = new InvalidStoreNameException();

            // Assert
            Assert.IsInstanceOfType(exception, typeof(System.Exception));
        }

        /// <summary>
        /// Verifies that the exception can be thrown and caught as InvalidStoreNameException.
        /// </summary>
        [TestMethod]
        public void Constructor_ThrownException_CanBeCaughtAsInvalidStoreNameException()
        {
            // Arrange
            var exceptionThrown = false;
            var correctMessage = false;

            // Act
            try
            {
                throw new InvalidStoreNameException();
            }
            catch (InvalidStoreNameException ex)
            {
                exceptionThrown = true;
                correctMessage = ex.Message == "Store name cannot be empty.";
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(correctMessage);
        }

        /// <summary>
        /// Verifies that the exception can be thrown and caught as DomainException.
        /// </summary>
        [TestMethod]
        public void Constructor_ThrownException_CanBeCaughtAsDomainException()
        {
            // Arrange
            var exceptionThrown = false;

            // Act
            try
            {
                throw new InvalidStoreNameException();
            }
            catch (DomainException)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
        }

        /// <summary>
        /// Verifies that the exception can be thrown and caught as a generic Exception.
        /// </summary>
        [TestMethod]
        public void Constructor_ThrownException_CanBeCaughtAsException()
        {
            // Arrange
            var exceptionThrown = false;

            // Act
            try
            {
                throw new InvalidStoreNameException();
            }
            catch (System.Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
        }

        /// <summary>
        /// Verifies that the exception message is not null or empty.
        /// </summary>
        [TestMethod]
        public void Constructor_NoParameters_MessageIsNotNullOrEmpty()
        {
            // Arrange & Act
            var exception = new InvalidStoreNameException();

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(exception.Message));
        }
    }
}