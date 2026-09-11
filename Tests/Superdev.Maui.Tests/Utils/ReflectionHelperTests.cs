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
            var fieldValue = ReflectionHelper.GetFieldValue<string>(obj, "InternalString");

            // Assert
            fieldValue.Should().Be(obj.InternalString);
        }

        [Fact]
        public void ShouldGetFieldValue_FromBaseClass()
        {
            // Arrange
            var obj = new MyClass(internalDateTime: new DateTime(2000, 1, 1), internalString: null);

            // Act
            var fieldValue = ReflectionHelper.GetFieldValue<DateTime>(obj, "InternalDateTime");

            // Assert
            fieldValue.Should().Be(obj.InternalDateTime);
        }

        [Fact]
        public void ShouldGetFieldValue_FieldDoesNotExist()
        {
            // Arrange
            var obj = new MyClass(internalDateTime: null, internalString: null);

            // Act
            Action action = () => ReflectionHelper.GetFieldValue<string>(obj, "NonExistentField");

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ShouldSetFieldValue()
        {
            // Arrange
            var obj = new MyClass(internalDateTime: null, internalString: "test value");

            // Act
            ReflectionHelper.SetFieldValue(obj, "InternalString", "new value");

            // Assert
            obj.InternalString.Should().Be("new value");
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
            const int paramA = 2;
            const int paramB = 3;
            var obj = new MyClass(internalDateTime: null, internalString: "test value");

            // Act
            var result = ReflectionHelper.RunMethod<int>(obj, "AddNumbers", paramA, paramB);

            // Assert
            result.Should().Be(5);
        }

        public class MyClass : MyBaseClass
        {
            public MyClass(DateTime? internalDateTime, string? internalString)
                : base(internalDateTime)
            {
                this.InternalString = internalString;
            }

            internal readonly string? InternalString;
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