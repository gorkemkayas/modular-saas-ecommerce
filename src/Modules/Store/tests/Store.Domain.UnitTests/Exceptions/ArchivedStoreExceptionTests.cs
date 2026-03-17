using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Domain.Exceptions;

namespace Store.Domain.Exceptions.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="ArchivedStoreException"/> class.
    /// </summary>
    [TestClass]
    public sealed class ArchivedStoreExceptionTests
    {
        /// <summary>
        /// Tests that the constructor with operation parameter creates an exception with the correctly formatted message.
        /// </summary>
        /// <param name="operation">The operation string to test.</param>
        /// <param name="expectedMessage">The expected exception message.</param>
        [TestMethod]
        [DataRow("modified", "Archived store cannot be modified.")]
        [DataRow("deleted", "Archived store cannot be deleted.")]
        [DataRow("updated", "Archived store cannot be updated.")]
        [DataRow("archived", "Archived store cannot be archived.")]
        public void Constructor_WithValidOperation_SetsCorrectMessage(string operation, string expectedMessage)
        {
            // Act
            var exception = new ArchivedStoreException(operation);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor handles an empty string operation correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_WithEmptyString_SetsMessageWithEmptyOperation()
        {
            // Arrange
            string operation = string.Empty;
            string expectedMessage = "Archived store cannot be .";

            // Act
            var exception = new ArchivedStoreException(operation);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor preserves whitespace in the operation string.
        /// </summary>
        /// <param name="operation">The whitespace operation string to test.</param>
        [TestMethod]
        [DataRow("   ")]
        [DataRow("\t")]
        [DataRow("\n")]
        [DataRow(" modified ")]
        public void Constructor_WithWhitespaceOperation_PreservesWhitespaceInMessage(string operation)
        {
            // Arrange
            string expectedMessage = $"Archived store cannot be {operation}.";

            // Act
            var exception = new ArchivedStoreException(operation);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor handles very long operation strings correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_WithVeryLongOperation_SetsMessageWithLongString()
        {
            // Arrange
            string operation = new string('a', 10000);
            string expectedMessage = $"Archived store cannot be {operation}.";

            // Act
            var exception = new ArchivedStoreException(operation);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor handles operation strings with special characters correctly.
        /// </summary>
        /// <param name="operation">The operation string with special characters.</param>
        [TestMethod]
        [DataRow("modi\"fied")]
        [DataRow("modi'fied")]
        [DataRow("modi$fied")]
        [DataRow("modified!@#$%^&*()")]
        [DataRow("<modified>")]
        [DataRow("modi\rfied")]
        [DataRow("modi\tfied")]
        public void Constructor_WithSpecialCharactersInOperation_SetsMessageCorrectly(string operation)
        {
            // Arrange
            string expectedMessage = $"Archived store cannot be {operation}.";

            // Act
            var exception = new ArchivedStoreException(operation);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor handles Unicode and international characters correctly.
        /// </summary>
        /// <param name="operation">The operation string with Unicode characters.</param>
        [TestMethod]
        [DataRow("удалено")]
        [DataRow("삭제됨")]
        [DataRow("modifié")]
        [DataRow("🔒locked")]
        public void Constructor_WithUnicodeCharacters_SetsMessageCorrectly(string operation)
        {
            // Arrange
            string expectedMessage = $"Archived store cannot be {operation}.";

            // Act
            var exception = new ArchivedStoreException(operation);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the constructor with null operation parameter throws ArgumentNullException
        /// or creates an exception with "Archived store cannot be ." message depending on runtime behavior.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullOperation_HandlesNullCorrectly()
        {
            // Arrange
            string? operation = null;

            // Act & Assert
            // In .NET, string interpolation with null produces empty string
            var exception = new ArchivedStoreException(operation!);
            Assert.IsNotNull(exception);
            Assert.AreEqual("Archived store cannot be .", exception.Message);
        }

        /// <summary>
        /// Tests that the created exception is of the correct type and inherits from DomainException.
        /// </summary>
        [TestMethod]
        public void Constructor_WithOperation_CreatesCorrectExceptionType()
        {
            // Arrange
            string operation = "modified";

            // Act
            var exception = new ArchivedStoreException(operation);

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ArchivedStoreException));
            Assert.IsInstanceOfType(exception, typeof(DomainException));
            Assert.IsInstanceOfType(exception, typeof(Exception));
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an exception with the correct message.
        /// Input: None (parameterless constructor).
        /// Expected: Exception is created with message "Archived store cannot be modified."
        /// </summary>
        [TestMethod]
        public void Constructor_WhenCalled_CreatesExceptionWithCorrectMessage()
        {
            // Arrange
            const string expectedMessage = "Archived store cannot be modified.";

            // Act
            var exception = new ArchivedStoreException();

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an instance of the correct type.
        /// Input: None (parameterless constructor).
        /// Expected: Exception is an instance of ArchivedStoreException.
        /// </summary>
        [TestMethod]
        public void Constructor_WhenCalled_CreatesInstanceOfArchivedStoreException()
        {
            // Act
            var exception = new ArchivedStoreException();

            // Assert
            Assert.IsInstanceOfType(exception, typeof(ArchivedStoreException));
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an instance that inherits from DomainException.
        /// Input: None (parameterless constructor).
        /// Expected: Exception is an instance of DomainException.
        /// </summary>
        [TestMethod]
        public void Constructor_WhenCalled_CreatesInstanceOfDomainException()
        {
            // Act
            var exception = new ArchivedStoreException();

            // Assert
            Assert.IsInstanceOfType(exception, typeof(DomainException));
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an instance that inherits from Exception.
        /// Input: None (parameterless constructor).
        /// Expected: Exception is an instance of Exception.
        /// </summary>
        [TestMethod]
        public void Constructor_WhenCalled_CreatesInstanceOfException()
        {
            // Act
            var exception = new ArchivedStoreException();

            // Assert
            Assert.IsInstanceOfType(exception, typeof(Exception));
        }

        /// <summary>
        /// Tests that the exception can be thrown and caught as ArchivedStoreException.
        /// Input: Throw ArchivedStoreException using parameterless constructor.
        /// Expected: Exception is caught successfully as ArchivedStoreException with correct message.
        /// </summary>
        [TestMethod]
        public void Constructor_WhenThrown_CanBeCaughtAsArchivedStoreException()
        {
            // Arrange
            const string expectedMessage = "Archived store cannot be modified.";
            ArchivedStoreException? caughtException = null;

            // Act
            try
            {
                throw new ArchivedStoreException();
            }
            catch (ArchivedStoreException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.IsNotNull(caughtException);
            Assert.AreEqual(expectedMessage, caughtException.Message);
        }

        /// <summary>
        /// Tests that the exception can be thrown and caught as DomainException.
        /// Input: Throw ArchivedStoreException using parameterless constructor.
        /// Expected: Exception is caught successfully as DomainException with correct message.
        /// </summary>
        [TestMethod]
        public void Constructor_WhenThrown_CanBeCaughtAsDomainException()
        {
            // Arrange
            const string expectedMessage = "Archived store cannot be modified.";
            DomainException? caughtException = null;

            // Act
            try
            {
                throw new ArchivedStoreException();
            }
            catch (DomainException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.IsNotNull(caughtException);
            Assert.AreEqual(expectedMessage, caughtException.Message);
            Assert.IsInstanceOfType(caughtException, typeof(ArchivedStoreException));
        }
    }
}