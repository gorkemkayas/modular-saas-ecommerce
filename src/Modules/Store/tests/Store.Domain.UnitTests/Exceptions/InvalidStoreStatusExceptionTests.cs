using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Domain;
using Store.Domain.Exceptions;

namespace Store.Domain.Exceptions.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="InvalidStoreStatusException"/> class.
    /// </summary>
    [TestClass]
    public sealed class InvalidStoreStatusExceptionTests
    {
        /// <summary>
        /// Tests that CannotPublish returns a non-null InvalidStoreStatusException instance.
        /// </summary>
        [TestMethod]
        public void CannotPublish_WhenCalled_ReturnsNonNullException()
        {
            // Act
            var exception = InvalidStoreStatusException.CannotPublish();

            // Assert
            Assert.IsNotNull(exception);
        }

        /// <summary>
        /// Tests that CannotPublish returns an exception of the correct type.
        /// </summary>
        [TestMethod]
        public void CannotPublish_WhenCalled_ReturnsInvalidStoreStatusException()
        {
            // Act
            var exception = InvalidStoreStatusException.CannotPublish();

            // Assert
            Assert.IsInstanceOfType<InvalidStoreStatusException>(exception);
        }

        /// <summary>
        /// Tests that CannotPublish returns an exception with the correct message.
        /// </summary>
        [TestMethod]
        public void CannotPublish_WhenCalled_ReturnsExceptionWithCorrectMessage()
        {
            // Arrange
            const string expectedMessage = "Only active stores can be published.";

            // Act
            var exception = InvalidStoreStatusException.CannotPublish();

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that CannotPublish returns an exception that is a DomainException.
        /// </summary>
        [TestMethod]
        public void CannotPublish_WhenCalled_ReturnsExceptionThatIsDomainException()
        {
            // Act
            var exception = InvalidStoreStatusException.CannotPublish();

            // Assert
            Assert.IsInstanceOfType<DomainException>(exception);
        }

        /// <summary>
        /// Tests that CannotPublish creates a new instance on each call.
        /// </summary>
        [TestMethod]
        public void CannotPublish_WhenCalledMultipleTimes_CreatesNewInstances()
        {
            // Act
            var exception1 = InvalidStoreStatusException.CannotPublish();
            var exception2 = InvalidStoreStatusException.CannotPublish();

            // Assert
            Assert.AreNotSame(exception1, exception2);
        }

        /// <summary>
        /// Tests that the constructor creates an exception with the provided message
        /// for various valid string inputs including empty, whitespace, long strings,
        /// and special characters.
        /// </summary>
        /// <param name="message">The message to pass to the constructor.</param>
        [TestMethod]
        [DataRow("Only active stores can be published.")]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("\t\n\r")]
        [DataRow("A very long message that contains many characters to test the handling of large input strings in the exception constructor. This message is intentionally verbose to ensure that the exception can properly store and retrieve messages of significant length without any truncation or data loss issues.")]
        [DataRow("Message with special characters: !@#$%^&*()_+-=[]{}|;':\"<>?,./~`")]
        [DataRow("Message with unicode: 你好世界 مرحبا بالعالم")]
        public void Constructor_WithValidMessage_CreatesExceptionWithMessage(string message)
        {
            // Arrange & Act
            var exception = new InvalidStoreStatusException(message);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(message, exception.Message);
            Assert.IsInstanceOfType(exception, typeof(InvalidStoreStatusException));
            Assert.IsInstanceOfType(exception, typeof(DomainException));
        }

        /// <summary>
        /// Tests that the constructor handles null message parameter.
        /// Given that the parameter is non-nullable, this tests runtime behavior
        /// when null is passed, which should be handled by the base exception class.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullMessage_CreatesException()
        {
            // Arrange
            string? message = null;

            // Act
            var exception = new InvalidStoreStatusException(message!);

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(InvalidStoreStatusException));
            Assert.IsInstanceOfType(exception, typeof(DomainException));
        }

        /// <summary>
        /// Tests that the constructor creates an exception that inherits from the correct
        /// base exception types in the inheritance hierarchy.
        /// </summary>
        [TestMethod]
        public void Constructor_CreatesExceptionWithCorrectInheritance_InheritsFromSystemException()
        {
            // Arrange
            var message = "Test message";

            // Act
            var exception = new InvalidStoreStatusException(message);

            // Assert
            Assert.IsInstanceOfType(exception, typeof(Exception));
            Assert.IsInstanceOfType(exception, typeof(DomainException));
            Assert.IsInstanceOfType(exception, typeof(InvalidStoreStatusException));
        }

        /// <summary>
        /// Tests that CannotActivate returns a non-null InvalidStoreStatusException instance
        /// with the correct message "Archived store cannot be activated.".
        /// </summary>
        [TestMethod]
        public void CannotActivate_WhenCalled_ReturnsExceptionWithCorrectMessage()
        {
            // Arrange
            const string expectedMessage = "Archived store cannot be activated.";

            // Act
            InvalidStoreStatusException result = InvalidStoreStatusException.CannotActivate();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedMessage, result.Message);
            Assert.IsInstanceOfType(result, typeof(InvalidStoreStatusException));
        }
    }
}