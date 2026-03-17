using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store.Domain.ValueObjects;

namespace Store.Domain.ValueObjects.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="Slug"/> class.
    /// </summary>
    [TestClass]
    public sealed class SlugTests
    {
        /// <summary>
        /// Tests that Create successfully creates a Slug with valid lowercase alphanumeric input.
        /// </summary>
        [TestMethod]
        [DataRow("test")]
        [DataRow("test123")]
        [DataRow("123test")]
        [DataRow("123")]
        [DataRow("abc")]
        [DataRow("a")]
        [DataRow("1")]
        [DataRow("a1b2c3")]
        public void Create_ValidLowercaseAlphanumeric_ReturnsSlug(string value)
        {
            // Act
            var result = Slug.Create(value);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(value, result.Value);
        }

        /// <summary>
        /// Tests that Create successfully creates a Slug with valid slug containing dashes.
        /// </summary>
        [TestMethod]
        [DataRow("test-slug")]
        [DataRow("test-slug-123")]
        [DataRow("my-awesome-product")]
        [DataRow("a-b-c-d-e")]
        [DataRow("test1-test2-test3")]
        [DataRow("abc-123")]
        [DataRow("123-abc")]
        public void Create_ValidSlugWithDashes_ReturnsSlug(string value)
        {
            // Act
            var result = Slug.Create(value);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(value, result.Value);
        }

        /// <summary>
        /// Tests that Create normalizes uppercase characters to lowercase.
        /// </summary>
        [TestMethod]
        [DataRow("TEST", "test")]
        [DataRow("Test", "test")]
        [DataRow("TeSt", "test")]
        [DataRow("TEST-SLUG", "test-slug")]
        [DataRow("Test-Slug", "test-slug")]
        [DataRow("MyAwesomeProduct", "myawesomeproduct")]
        [DataRow("ABC-123", "abc-123")]
        public void Create_UppercaseInput_NormalizesToLowercase(string input, string expected)
        {
            // Act
            var result = Slug.Create(input);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result.Value);
        }

        /// <summary>
        /// Tests that Create trims leading and trailing whitespace before validation.
        /// </summary>
        [TestMethod]
        [DataRow("  test  ", "test")]
        [DataRow("\ttest\t", "test")]
        [DataRow(" test-slug ", "test-slug")]
        [DataRow("  test123  ", "test123")]
        [DataRow("\n\rtest\n\r", "test")]
        public void Create_InputWithWhitespace_TrimsAndCreatesSlug(string input, string expected)
        {
            // Act
            var result = Slug.Create(input);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result.Value);
        }

        /// <summary>
        /// Tests that Create successfully creates a Slug with very long valid input.
        /// </summary>
        [TestMethod]
        public void Create_VeryLongValidSlug_ReturnsSlug()
        {
            // Arrange
            string value = "this-is-a-very-long-slug-with-many-segments-separated-by-dashes-to-test-the-behavior-with-extremely-long-input-strings";

            // Act
            var result = Slug.Create(value);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(value, result.Value);
        }

        /// <summary>
        /// Tests that Create returns a Slug with correct Value property.
        /// </summary>
        [TestMethod]
        public void Create_ValidInput_SlugValueMatchesNormalizedInput()
        {
            // Arrange
            string input = "My-Product-123";
            string expected = "my-product-123";

            // Act
            var result = Slug.Create(input);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result.Value);
            Assert.AreEqual(expected, result.ToString());
        }

        /// <summary>
        /// Tests that two slugs created from the same normalized value are equal.
        /// </summary>
        [TestMethod]
        public void Create_SameNormalizedValue_ProducesSlugsWithEqualValues()
        {
            // Arrange
            string input1 = "test-slug";
            string input2 = "TEST-SLUG";
            string input3 = " Test-Slug ";

            // Act
            var slug1 = Slug.Create(input1);
            var slug2 = Slug.Create(input2);
            var slug3 = Slug.Create(input3);

            // Assert
            Assert.AreEqual(slug1.Value, slug2.Value);
            Assert.AreEqual(slug2.Value, slug3.Value);
            Assert.AreEqual("test-slug", slug1.Value);
        }

        /// <summary>
        /// Tests that Create handles numeric-only slugs correctly.
        /// </summary>
        [TestMethod]
        [DataRow("0")]
        [DataRow("123456789")]
        [DataRow("42")]
        [DataRow("999")]
        [DataRow("1-2-3-4")]
        [DataRow("12-34-56")]
        public void Create_NumericOnlySlug_ReturnsSlug(string value)
        {
            // Act
            var result = Slug.Create(value);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(value, result.Value);
        }

        /// <summary>
        /// Tests that Create handles single character slugs correctly.
        /// </summary>
        [TestMethod]
        [DataRow("a")]
        [DataRow("z")]
        [DataRow("0")]
        [DataRow("9")]
        [DataRow("A", "a")]
        [DataRow("Z", "z")]
        public void Create_SingleCharacter_ReturnsSlug(string input, string? expected = null)
        {
            // Arrange
            expected ??= input;

            // Act
            var result = Slug.Create(input);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result.Value);
        }

        /// <summary>
        /// Tests that ToString returns the normalized slug value for various valid input strings.
        /// </summary>
        /// <param name="input">The input string to create the slug.</param>
        /// <param name="expected">The expected string value returned by ToString.</param>
        [TestMethod]
        [DataRow("test", "test", DisplayName = "Simple lowercase slug")]
        [DataRow("test-slug", "test-slug", DisplayName = "Slug with hyphens")]
        [DataRow("test123", "test123", DisplayName = "Slug with numbers")]
        [DataRow("my-product-123", "my-product-123", DisplayName = "Complex valid slug")]
        [DataRow("TEST", "test", DisplayName = "Uppercase normalized to lowercase")]
        [DataRow("Test-Slug", "test-slug", DisplayName = "Mixed case normalized")]
        [DataRow(" test ", "test", DisplayName = "Trimmed spaces")]
        [DataRow(" Test-Slug-123 ", "test-slug-123", DisplayName = "Trimmed and normalized")]
        [DataRow("a", "a", DisplayName = "Single character")]
        [DataRow("a1", "a1", DisplayName = "Two characters")]
        [DataRow("abc-def-ghi-jkl-mno-pqr", "abc-def-ghi-jkl-mno-pqr", DisplayName = "Long slug with multiple hyphens")]
        [DataRow("product-1-2-3", "product-1-2-3", DisplayName = "Multiple numbers separated by hyphens")]
        public void ToString_ValidSlug_ReturnsNormalizedValue(string input, string expected)
        {
            // Arrange
            var slug = Slug.Create(input);

            // Act
            var result = slug.ToString();

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that ToString returns the same value as the Value property.
        /// </summary>
        [TestMethod]
        public void ToString_ValidSlug_ReturnsSameAsValueProperty()
        {
            // Arrange
            var slug = Slug.Create("test-slug");

            // Act
            var toStringResult = slug.ToString();
            var valueProperty = slug.Value;

            // Assert
            Assert.AreEqual(valueProperty, toStringResult);
        }
    }
}