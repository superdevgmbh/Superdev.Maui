using Superdev.Maui.Utils;

namespace Superdev.Maui.Tests.Utils
{
    public class ReflectionHelperTests
    {
        [Fact]
        public void ShouldGetPropertyValue()
        {
            // Arrange
            var obj = new MyClass(internalDateTime: null, internalString: "test value");

            // Act
            var propertyValue = ReflectionHelper.GetPropertyValue<string>(obj, "InternalString");

            // Assert
            Assert.Equal(obj.InternalString, propertyValue);
        }

        [Fact]
        public void ShouldGetPropertyValue_FromBaseClass()
        {
            // Arrange
            var obj = new MyClass(internalDateTime: new DateTime(2000, 1, 1), internalString: null);

            // Act
            var propertyValue = ReflectionHelper.GetPropertyValue<DateTime>(obj, "InternalDateTime");

            // Assert
            Assert.Equal(obj.InternalDateTime, propertyValue);
        }

        [Fact]
        public void ShouldGetPropertyValue_PropertyDoesNotExist()
        {
            // Arrange
            var obj = new MyClass(internalDateTime: null, internalString: null);

            // Act
            Action action = () => ReflectionHelper.GetPropertyValue<string>(obj, "NonExistentProperty");

            // Assert
            Assert.Throws<ArgumentException>(action);
        }

        public class MyClass : MyBaseClass
        {
            public MyClass(DateTime? internalDateTime, string internalString)
                : base(internalDateTime)
            {
                this.InternalString = internalString;
            }

            internal string InternalString { get; }
        }

        public class MyBaseClass
        {
            protected MyBaseClass(DateTime? internalDateTime)
            {
                this.InternalDateTime = internalDateTime;
            }

            internal DateTime? InternalDateTime { get; }
        }
    }
}