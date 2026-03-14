using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Domain.Exceptions;

namespace Store.Domain.Exceptions.UnitTests
{
    [TestClass]
    public class DomainExceptionTests
    {
        /// <summary>
        /// Tests that the constructor properly initializes the exception with a valid message and inner exception.
        /// Input: Valid message string and valid Exception instance.
        /// Expected: Message and InnerException properties are correctly set.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidMessageAndInnerException_SetsPropertiesCorrectly()
        {
            // Arrange
            string expectedMessage = "Test error message";
            Exception expectedInnerException = new InvalidOperationException("Inner exception");

            // Act
            var exception = new TestDomainException(expectedMessage, expectedInnerException);

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
            Assert.AreSame(expectedInnerException, exception.InnerException);
        }

        /// <summary>
        /// Tests that the constructor handles null message parameter.
        /// Input: null message and valid inner exception.
        /// Expected: Exception is created with null message and valid InnerException.
        /// </summary>
        [TestMethod]
        public void Constructor_NullMessage_SetsMessageToNull()
        {
            // Arrange
            string? message = null;
            Exception innerException = new ArgumentException("Inner exception");

            // Act
            var exception = new TestDomainException(message!, innerException);

            // Assert
            Assert.IsNotNull(exception.Message); // Exception class provides a default message when null is passed
            Assert.AreSame(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests that the constructor handles null inner exception parameter.
        /// Input: Valid message and null inner exception.
        /// Expected: Exception is created with valid message and null InnerException.
        /// </summary>
        [TestMethod]
        public void Constructor_NullInnerException_SetsInnerExceptionToNull()
        {
            // Arrange
            string message = "Test error message";
            Exception? innerException = null;

            // Act
            var exception = new TestDomainException(message, innerException!);

            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.IsNull(exception.InnerException);
        }

        /// <summary>
        /// Tests that the constructor handles both null message and null inner exception.
        /// Input: null message and null inner exception.
        /// Expected: Exception is created with both properties null.
        /// </summary>
        [TestMethod]
        public void Constructor_NullMessageAndNullInnerException_SetsBothPropertiesToNull()
        {
            // Arrange
            string? message = null;
            Exception? innerException = null;

            // Act
            var exception = new TestDomainException(message!, innerException!);

            // Assert
            Assert.IsNotNull(exception.Message); // Exception class provides a default message when null is passed
            Assert.IsNull(exception.InnerException);
        }

        /// <summary>
        /// Tests that the constructor handles various string edge cases for the message parameter.
        /// Input: Empty string, whitespace string, or very long string as message.
        /// Expected: Exception is created with the exact message provided.
        /// </summary>
        [TestMethod]
        [DataRow("", DisplayName = "Empty string message")]
        [DataRow("   ", DisplayName = "Whitespace only message")]
        [DataRow("This is a very long error message that contains a lot of text to test the handling of extremely long strings in exception messages. This should not cause any issues but it's important to verify that the constructor properly handles such cases without truncation or errors.", DisplayName = "Very long message")]
        public void Constructor_EdgeCaseMessages_SetsMessageCorrectly(string message)
        {
            // Arrange
            Exception innerException = new Exception("Inner exception");

            // Act
            var exception = new TestDomainException(message, innerException);

            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreSame(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests that the constructor handles different types of inner exceptions.
        /// Input: Valid message and various exception types as inner exception.
        /// Expected: Exception is created with the exact inner exception provided.
        /// </summary>
        [TestMethod]
        public void Constructor_DifferentInnerExceptionTypes_SetsInnerExceptionCorrectly()
        {
            // Arrange
            string message = "Test error";
            Exception[] innerExceptions = new Exception[]
            {
                new ArgumentException("Argument error"),
                new InvalidOperationException("Invalid operation"),
                new NullReferenceException("Null reference"),
                new NotImplementedException("Not implemented")
            };

            // Act & Assert
            foreach (var innerException in innerExceptions)
            {
                var exception = new TestDomainException(message, innerException);
                Assert.AreSame(innerException, exception.InnerException);
            }
        }

        /// <summary>
        /// Tests that the constructor handles special characters in the message.
        /// Input: Message with special characters, newlines, tabs, and unicode.
        /// Expected: Exception is created with the exact message provided.
        /// </summary>
        [TestMethod]
        public void Constructor_MessageWithSpecialCharacters_SetsMessageCorrectly()
        {
            // Arrange
            string message = "Error with special chars: \n\t\r!@#$%^&*(){}[]<>?/\\|`~\"'";
            Exception innerException = new Exception("Inner");

            // Act
            var exception = new TestDomainException(message, innerException);

            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreSame(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests that the constructor handles nested exceptions (exception with inner exception).
        /// Input: Message and an inner exception that itself has an inner exception.
        /// Expected: Exception is created with proper exception chain.
        /// </summary>
        [TestMethod]
        public void Constructor_NestedInnerException_PreservesExceptionChain()
        {
            // Arrange
            string message = "Outer exception";
            Exception deepestException = new ArgumentException("Deepest exception");
            Exception middleException = new InvalidOperationException("Middle exception", deepestException);

            // Act
            var exception = new TestDomainException(message, middleException);

            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreSame(middleException, exception.InnerException);
            Assert.AreSame(deepestException, exception.InnerException?.InnerException);
        }

        /// <summary>
        /// Concrete test implementation of the abstract DomainException class.
        /// Used exclusively for testing the DomainException constructors.
        /// </summary>
        private class TestDomainException : DomainException
        {
            public TestDomainException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }

        /// <summary>
        /// Tests that the constructor creates an exception with a valid message.
        /// Input: A standard error message string.
        /// Expected: Exception is created successfully with the message property set correctly.
        /// </summary>
        [TestMethod]
        [DataRow("An error occurred")]
        [DataRow("Domain validation failed")]
        [DataRow("Test error message")]
        public void DomainException_WithValidMessage_CreatesExceptionSuccessfully(string message)
        {
            // Arrange & Act
            var exception = new ConcreteDomainException(message);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(message, exception.Message);
            Assert.IsInstanceOfType(exception, typeof(DomainException));
            Assert.IsInstanceOfType(exception, typeof(Exception));
        }

        /// <summary>
        /// Tests that the constructor handles a null message.
        /// Input: null message.
        /// Expected: Exception is created successfully with null message (base Exception behavior).
        /// </summary>
        [TestMethod]
        public void DomainException_WithNullMessage_CreatesExceptionSuccessfully()
        {
            // Arrange & Act
            var exception = new ConcreteDomainException(null!);

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsNotNull(exception.Message); // Exception class provides a default message
        }

        /// <summary>
        /// Tests that the constructor handles an empty string message.
        /// Input: Empty string.
        /// Expected: Exception is created successfully with an empty message.
        /// </summary>
        [TestMethod]
        public void DomainException_WithEmptyMessage_CreatesExceptionSuccessfully()
        {
            // Arrange
            string message = string.Empty;

            // Act
            var exception = new ConcreteDomainException(message);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(string.Empty, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor handles whitespace-only messages.
        /// Input: Whitespace-only strings.
        /// Expected: Exception is created successfully with the whitespace message.
        /// </summary>
        [TestMethod]
        [DataRow("   ")]
        [DataRow("\t")]
        [DataRow("\n")]
        [DataRow("\r\n")]
        [DataRow(" \t\n ")]
        public void DomainException_WithWhitespaceMessage_CreatesExceptionSuccessfully(string message)
        {
            // Arrange & Act
            var exception = new ConcreteDomainException(message);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(message, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor handles very long messages.
        /// Input: A very long string message.
        /// Expected: Exception is created successfully with the full long message.
        /// </summary>
        [TestMethod]
        public void DomainException_WithVeryLongMessage_CreatesExceptionSuccessfully()
        {
            // Arrange
            string message = new string('A', 10000);

            // Act
            var exception = new ConcreteDomainException(message);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(10000, exception.Message.Length);
        }

        /// <summary>
        /// Tests that the constructor handles messages with special characters.
        /// Input: Strings with special, control, and Unicode characters.
        /// Expected: Exception is created successfully preserving all special characters.
        /// </summary>
        [TestMethod]
        [DataRow("Error: <tag> & \"quoted\" 'text'")]
        [DataRow("Path\\to\\file")]
        [DataRow("Line1\nLine2\rLine3")]
        [DataRow("Unicode: é, ñ, 中文, 日本語")]
        [DataRow("Special: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
        [DataRow("\0\a\b\f\v")]
        public void DomainException_WithSpecialCharacters_CreatesExceptionSuccessfully(string message)
        {
            // Arrange & Act
            var exception = new ConcreteDomainException(message);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(message, exception.Message);
        }

        /// <summary>
        /// Tests that the exception can be thrown and caught correctly.
        /// Input: A standard error message.
        /// Expected: Exception is thrown and can be caught as DomainException and Exception.
        /// </summary>
        [TestMethod]
        public void DomainException_WhenThrown_CanBeCaughtAsDomainException()
        {
            // Arrange
            string message = "Test exception message";
            DomainException? caughtException = null;

            // Act
            try
            {
                throw new ConcreteDomainException(message);
            }
            catch (DomainException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.IsNotNull(caughtException);
            Assert.AreEqual(message, caughtException.Message);
        }

        /// <summary>
        /// Tests that the exception can be caught as a base Exception type.
        /// Input: A standard error message.
        /// Expected: Exception is thrown and can be caught as Exception.
        /// </summary>
        [TestMethod]
        public void DomainException_WhenThrown_CanBeCaughtAsException()
        {
            // Arrange
            string message = "Test exception message";
            Exception? caughtException = null;

            // Act
            try
            {
                throw new ConcreteDomainException(message);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.IsNotNull(caughtException);
            Assert.AreEqual(message, caughtException.Message);
            Assert.IsInstanceOfType(caughtException, typeof(DomainException));
        }

        /// <summary>
        /// Concrete implementation of DomainException for testing purposes.
        /// This helper class exposes the protected constructor to enable testing.
        /// </summary>
        private class ConcreteDomainException : DomainException
        {
            public ConcreteDomainException(string message) : base(message)
            {
            }
        }
    }
}