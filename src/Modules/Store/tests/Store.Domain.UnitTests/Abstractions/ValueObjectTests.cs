using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Domain.Abstractions;

namespace Store.Domain.Abstractions.UnitTests
{
    /// <summary>
    /// Unit tests for the ValueObject abstract class.
    /// </summary>
    [TestClass]
    public class ValueObjectTests
    {
        /// <summary>
        /// Tests that Equals returns false when the parameter is null.
        /// </summary>
        [TestMethod]
        public void Equals_NullObject_ReturnsFalse()
        {
            // Arrange
            var valueObject = new TestValueObject(1, "test");

            // Act
            bool result = valueObject.Equals(null);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that Equals returns false when the parameter is of a different type.
        /// </summary>
        [TestMethod]
        public void Equals_DifferentType_ReturnsFalse()
        {
            // Arrange
            var valueObject = new TestValueObject(1, "test");
            var differentObject = new object();

            // Act
            bool result = valueObject.Equals(differentObject);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that Equals returns false when comparing different ValueObject subtype.
        /// </summary>
        [TestMethod]
        public void Equals_DifferentValueObjectSubtype_ReturnsFalse()
        {
            // Arrange
            var valueObject1 = new TestValueObject(1, "test");
            var valueObject2 = new AnotherTestValueObject(1, "test");

            // Act
            bool result = valueObject1.Equals(valueObject2);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that Equals returns true when comparing the same object instance.
        /// </summary>
        [TestMethod]
        public void Equals_SameObjectInstance_ReturnsTrue()
        {
            // Arrange
            var valueObject = new TestValueObject(1, "test");

            // Act
            bool result = valueObject.Equals(valueObject);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that Equals returns true when comparing two objects with equal equality components.
        /// </summary>
        [TestMethod]
        public void Equals_EqualComponents_ReturnsTrue()
        {
            // Arrange
            var valueObject1 = new TestValueObject(1, "test");
            var valueObject2 = new TestValueObject(1, "test");

            // Act
            bool result = valueObject1.Equals(valueObject2);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that Equals returns false when comparing two objects with different equality components.
        /// </summary>
        [TestMethod]
        public void Equals_DifferentComponents_ReturnsFalse()
        {
            // Arrange
            var valueObject1 = new TestValueObject(1, "test");
            var valueObject2 = new TestValueObject(2, "different");

            // Act
            bool result = valueObject1.Equals(valueObject2);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that Equals returns true when both objects have empty equality components.
        /// </summary>
        [TestMethod]
        public void Equals_EmptyComponents_ReturnsTrue()
        {
            // Arrange
            var valueObject1 = new EmptyValueObject();
            var valueObject2 = new EmptyValueObject();

            // Act
            bool result = valueObject1.Equals(valueObject2);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that Equals returns false when one component differs among multiple components.
        /// </summary>
        [TestMethod]
        public void Equals_OneComponentDifferent_ReturnsFalse()
        {
            // Arrange
            var valueObject1 = new TestValueObject(1, "test");
            var valueObject2 = new TestValueObject(1, "different");

            // Act
            bool result = valueObject1.Equals(valueObject2);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that Equals returns false when objects have different number of equality components.
        /// </summary>
        [TestMethod]
        public void Equals_DifferentNumberOfComponents_ReturnsFalse()
        {
            // Arrange
            var valueObject1 = new TestValueObject(1, "test");
            var valueObject2 = new SingleComponentValueObject(1);

            // Act
            bool result = valueObject1.Equals(valueObject2);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that Equals correctly handles null values in equality components.
        /// </summary>
        [TestMethod]
        public void Equals_NullComponentValues_ReturnsTrue()
        {
            // Arrange
            var valueObject1 = new NullableComponentValueObject(null);
            var valueObject2 = new NullableComponentValueObject(null);

            // Act
            bool result = valueObject1.Equals(valueObject2);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that Equals returns false when one object has null component and another has non-null.
        /// </summary>
        [TestMethod]
        public void Equals_OneNullComponentOneNonNull_ReturnsFalse()
        {
            // Arrange
            var valueObject1 = new NullableComponentValueObject(null);
            var valueObject2 = new NullableComponentValueObject("test");

            // Act
            bool result = valueObject1.Equals(valueObject2);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that Equals works correctly with complex objects as components.
        /// </summary>
        [TestMethod]
        public void Equals_ComplexObjectComponents_ReturnsTrue()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3 };
            var valueObject1 = new ComplexComponentValueObject(list, "test");
            var valueObject2 = new ComplexComponentValueObject(list, "test");

            // Act
            bool result = valueObject1.Equals(valueObject2);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that Equals returns false when complex object components differ.
        /// </summary>
        [TestMethod]
        public void Equals_ComplexObjectComponentsDifferent_ReturnsFalse()
        {
            // Arrange
            var valueObject1 = new ComplexComponentValueObject(new List<int> { 1, 2, 3 }, "test");
            var valueObject2 = new ComplexComponentValueObject(new List<int> { 1, 2, 4 }, "test");

            // Act
            bool result = valueObject1.Equals(valueObject2);

            // Assert
            Assert.IsFalse(result);
        }

        #region Helper Classes

        private class TestValueObject : ValueObject
        {
            private readonly int _id;
            private readonly string _name;

            public TestValueObject(int id, string name)
            {
                _id = id;
                _name = name;
            }

            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return _id;
                yield return _name;
            }
        }

        private class AnotherTestValueObject : ValueObject
        {
            private readonly int _id;
            private readonly string _name;

            public AnotherTestValueObject(int id, string name)
            {
                _id = id;
                _name = name;
            }

            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return _id;
                yield return _name;
            }
        }

        private class EmptyValueObject : ValueObject
        {
            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield break;
            }
        }

        private class SingleComponentValueObject : ValueObject
        {
            private readonly int _value;

            public SingleComponentValueObject(int value)
            {
                _value = value;
            }

            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return _value;
            }
        }

        private class NullableComponentValueObject : ValueObject
        {
            private readonly string? _value;

            public NullableComponentValueObject(string? value)
            {
                _value = value;
            }

            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return _value!;
            }
        }

        private class ComplexComponentValueObject : ValueObject
        {
            private readonly List<int> _list;
            private readonly string _name;

            public ComplexComponentValueObject(List<int> list, string name)
            {
                _list = list;
                _name = name;
            }

            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return _list;
                yield return _name;
            }
        }

        #endregion

    }
}