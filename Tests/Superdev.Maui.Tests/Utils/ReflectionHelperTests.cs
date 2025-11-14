using Superdev.Maui.Utils;

namespace Superdev.Maui.Tests.Utils
{
    public class ReflectionHelperTests
    {
        [Fact]
        public void ShouldGetFieldValue()
        {
            // Arrange
            var obj = new MyClass(internalDateTime: null, internalString: "test value");

            // Act
            var FieldValue = ReflectionHelper.GetFieldValue<string>(obj, "InternalString");

            // Assert
            Assert.Equal(obj.InternalString, FieldValue);
        }

        [Fact]
        public void ShouldGetFieldValue_FromBaseClass()
        {
            // Arrange
            var obj = new MyClass(internalDateTime: new DateTime(2000, 1, 1), internalString: null);

            // Act
            var FieldValue = ReflectionHelper.GetFieldValue<DateTime>(obj, "InternalDateTime");

            // Assert
            Assert.Equal(obj.InternalDateTime, FieldValue);
        }

        [Fact]
        public void ShouldGetFieldValue_FieldDoesNotExist()
        {
            // Arrange
            var obj = new MyClass(internalDateTime: null, internalString: null);

            // Act
            Action action = () => ReflectionHelper.GetFieldValue<string>(obj, "NonExistentField");

            // Assert
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void ShouldSetFieldValue()
        {
            // Arrange
            var obj = new MyClass(internalDateTime: null, internalString: "test value");

            // Act
            ReflectionHelper.SetFieldValue(obj, "InternalString", "new value");

            // Assert
            Assert.Equal(obj.InternalString, "new value");
        }

        [Fact]
        public void ShouldRunMethod_WithoutParameters_WithoutReturnResult()
        {
            // Arrange
            var internalDateTime = new DateTime(2000, 1, 1);
            var obj = new MyClass(internalDateTime, internalString: null);

            // Act
            ReflectionHelper.RunMethod(obj, "Increment");
            ReflectionHelper.RunMethod(obj, "Increment");

            // Assert
            obj.Counter.Should().Be(2);
        }

        [Fact]
        public void ShouldRunMethod_WithoutParameters_WithReturnResult()
        {
            // Arrange
            var internalDateTime = new DateTime(2000, 1, 1);
            var obj = new MyClass(internalDateTime, internalString: "test value");

            // Act
            var result = ReflectionHelper.RunMethod<DateTime?>(obj, "GetInternalDateTime");

            // Assert
            result.Should().Be(internalDateTime);
        }

        [Fact]
        public void ShouldRunMethod_WithParameters_WithReturnResult()
        {
            // Arrange
            var paramA = 2;
            var paramB = 3;
            var obj = new MyClass(internalDateTime: null, internalString: "test value");

            // Act
            var result = ReflectionHelper.RunMethod<int>(obj, "AddNumbers", paramA, paramB);

            // Assert
            result.Should().Be(5);
        }

        public class MyClass : MyBaseClass
        {
            public MyClass(DateTime? internalDateTime, string internalString)
                : base(internalDateTime)
            {
                this.InternalString = internalString;
            }

            internal readonly string InternalString;
        }

        public class MyBaseClass
        {
            protected MyBaseClass(DateTime? internalDateTime)
            {
                this.InternalDateTime = internalDateTime;
            }

            internal DateTime? InternalDateTime;

            internal DateTime? GetInternalDateTime()
            {
                return this.InternalDateTime;
            }

            internal int AddNumbers(int a, int b)
            {
                return a + b;
            }

            public int Counter { get; set; }

            internal void Increment()
            {
                this.Counter++;
            }
        }
    }
}