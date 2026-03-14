using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Application.Exceptions;

namespace Store.Application.Exceptions.UnitTests
{
    /// <summary>
    /// Unit tests for the ApplicationException class.
    /// </summary>
    [TestClass]
    public class ApplicationExceptionTests
    {
        /// <summary>
        /// Tests that the constructor with message and inner exception properly sets the Message and InnerException properties
        /// for various valid input combinations.
        /// </summary>
        /// <param name="message">The error message to pass to the constructor.</param>
        /// <param name="innerExceptionMessage">The message for the inner exception, or null if no inner exception should be created.</param>
        [TestMethod]
        [DataRow("Test error message", "Inner exception message", DisplayName = "Normal message and inner exception")]
        [DataRow("", "Inner exception message", DisplayName = "Empty message with inner exception")]
        [DataRow("   ", "Inner exception message", DisplayName = "Whitespace message with inner exception")]
        [DataRow("Test error message with special characters: !@#$%^&*()", "Inner exception", DisplayName = "Message with special characters")]
        [DataRow("A very long error message that contains a lot of text to test how the exception handles large strings. This message is intentionally verbose to ensure that there are no issues with string length limits in the exception handling mechanism.", "Inner exception", DisplayName = "Very long message")]
        public void Constructor_WithMessageAndInnerException_SetsPropertiesCorrectly(string message, string innerExceptionMessage)
        {
            // Arrange
            var innerException = new InvalidOperationException(innerExceptionMessage);

            // Act
            var exception = new TestApplicationException(message, innerException);

            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreSame(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests that the constructor with null message properly handles the null value
        /// and sets the InnerException property correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullMessage_SetsInnerExceptionCorrectly()
        {
            // Arrange
            string? message = null;
            var innerException = new InvalidOperationException("Inner exception");

            // Act
            var exception = new TestApplicationException(message!, innerException);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreSame(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests that the constructor with null inner exception properly handles the null value
        /// and sets the Message property correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullInnerException_SetsMessageCorrectly()
        {
            // Arrange
            var message = "Test error message";
            Exception? innerException = null;

            // Act
            var exception = new TestApplicationException(message, innerException!);

            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.IsNull(exception.InnerException);
        }

        /// <summary>
        /// Tests that the constructor with both null message and null inner exception
        /// creates a valid exception instance.
        /// </summary>
        [TestMethod]
        public void Constructor_WithBothParametersNull_CreatesValidException()
        {
            // Arrange
            string? message = null;
            Exception? innerException = null;

            // Act
            var exception = new TestApplicationException(message!, innerException!);

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsNull(exception.InnerException);
        }

        /// <summary>
        /// Tests that the constructor properly handles different types of inner exceptions.
        /// </summary>
        /// <param name="exceptionType">The type of inner exception to test.</param>
        [TestMethod]
        [DataRow("ArgumentException", DisplayName = "ArgumentException as inner exception")]
        [DataRow("InvalidOperationException", DisplayName = "InvalidOperationException as inner exception")]
        [DataRow("NullReferenceException", DisplayName = "NullReferenceException as inner exception")]
        [DataRow("NotSupportedException", DisplayName = "NotSupportedException as inner exception")]
        public void Constructor_WithDifferentInnerExceptionTypes_SetsInnerExceptionCorrectly(string exceptionType)
        {
            // Arrange
            var message = "Test error message";
            Exception innerException = exceptionType switch
            {
                "ArgumentException" => new ArgumentException("Argument error"),
                "InvalidOperationException" => new InvalidOperationException("Operation error"),
                "NullReferenceException" => new NullReferenceException("Null reference error"),
                "NotSupportedException" => new NotSupportedException("Not supported error"),
                _ => throw new ArgumentException($"Unknown exception type: {exceptionType}")
            };

            // Act
            var exception = new TestApplicationException(message, innerException);

            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreSame(innerException, exception.InnerException);
            Assert.IsInstanceOfType(exception.InnerException, innerException.GetType());
        }

        /// <summary>
        /// Concrete test implementation of ApplicationException for testing purposes.
        /// </summary>
        private class TestApplicationException : ApplicationException
        {
            public TestApplicationException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }

        /// <summary>
        /// Tests that the ApplicationException constructor correctly initializes the exception
        /// with a normal, non-empty message.
        /// </summary>
        [TestMethod]
        [DataRow("An error occurred")]
        [DataRow("Error message")]
        [DataRow("Invalid operation")]
        public void ApplicationException_WithValidMessage_SetsMessageProperty(string message)
        {
            // Arrange & Act
            var exception = new TestableApplicationException(message);

            // Assert
            Assert.AreEqual(message, exception.Message);
        }

        /// <summary>
        /// Tests that the ApplicationException constructor correctly handles an empty string message.
        /// </summary>
        [TestMethod]
        public void ApplicationException_WithEmptyString_SetsEmptyMessage()
        {
            // Arrange
            string message = string.Empty;

            // Act
            var exception = new TestableApplicationException(message);

            // Assert
            Assert.AreEqual(string.Empty, exception.Message);
        }

        /// <summary>
        /// Tests that the ApplicationException constructor correctly handles a whitespace-only message.
        /// </summary>
        [TestMethod]
        [DataRow("   ")]
        [DataRow("\t")]
        [DataRow("\n")]
        [DataRow("\r\n")]
        public void ApplicationException_WithWhitespaceMessage_SetsWhitespaceMessage(string message)
        {
            // Arrange & Act
            var exception = new TestableApplicationException(message);

            // Assert
            Assert.AreEqual(message, exception.Message);
        }

        /// <summary>
        /// Tests that the ApplicationException constructor correctly handles a very long message string.
        /// </summary>
        [TestMethod]
        public void ApplicationException_WithVeryLongMessage_SetsLongMessage()
        {
            // Arrange
            string message = new string('A', 10000);

            // Act
            var exception = new TestableApplicationException(message);

            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(10000, exception.Message.Length);
        }

        /// <summary>
        /// Tests that the ApplicationException constructor correctly handles messages with special characters.
        /// </summary>
        [TestMethod]
        [DataRow("Error with special chars: !@#$%^&*()")]
        [DataRow("Error with quotes: \"quoted\" and 'single'")]
        [DataRow("Error with unicode: café, naïve, 日本語")]
        [DataRow("Error with control chars: \t\n\r")]
        public void ApplicationException_WithSpecialCharacters_SetsMessageWithSpecialChars(string message)
        {
            // Arrange & Act
            var exception = new TestableApplicationException(message);

            // Assert
            Assert.AreEqual(message, exception.Message);
        }

        /// <summary>
        /// Tests that the ApplicationException constructor handles null message gracefully,
        /// deferring to base Exception behavior which accepts nullable strings.
        /// </summary>
        [TestMethod]
        public void ApplicationException_WithNullMessage_SetsNullMessage()
        {
            // Arrange
            string? message = null;

            // Act
            var exception = new TestableApplicationException(message!);

            // Assert
            Assert.IsNotNull(exception.Message);
            Assert.IsFalse(string.IsNullOrWhiteSpace(exception.Message));
        }

        /// <summary>
        /// Concrete test implementation of ApplicationException to enable testing of the protected constructor.
        /// </summary>
        private class TestableApplicationException : ApplicationException
        {
            public TestableApplicationException(string message) : base(message)
            {
            }
        }
    }
}