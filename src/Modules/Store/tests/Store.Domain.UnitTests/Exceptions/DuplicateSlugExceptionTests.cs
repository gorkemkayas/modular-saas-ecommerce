using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Domain.Exceptions;


namespace Store.Domain.Exceptions.UnitTests
{
    /// <summary>
    /// Contains unit tests for the <see cref="DuplicateSlugException"/> class.
    /// </summary>
    [TestClass]
    public sealed class DuplicateSlugExceptionTests
    {
        /// <summary>
        /// Tests that the parameterless constructor creates an instance with the expected error message.
        /// </summary>
        [TestMethod]
        public void Constructor_NoParameters_CreatesInstanceWithExpectedMessage()
        {
            // Arrange
            const string expectedMessage = "New slug cannot be the same as current slug.";

            // Act
            var exception = new DuplicateSlugException();

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the exception is an instance of DomainException.
        /// </summary>
        [TestMethod]
        public void Constructor_NoParameters_CreatesInstanceOfDomainException()
        {
            // Act
            var exception = new DuplicateSlugException();

            // Assert
            Assert.IsInstanceOfType(exception, typeof(DomainException));
        }

    }
}