using Superdev.Maui.Extensions;

namespace Superdev.Maui.Tests.Extensions
{
    public class TypeExtensionsTests
    {
        #region GetDefaultValue Tests

        [Fact]
        public void ShouldGetDefaultValueForValueType()
        {
            // Arrange
            const int integerValue = 1;

            // Act
            var defaultValue = integerValue.GetType().GetDefaultValue();

            // Assert
            defaultValue.Should().Be(0);
        }

        [Fact]
        public void ShouldGetDefaultValueForReferenceType()
        {
            // Arrange
            var person = new Person();

            // Act
            var defaultValue = person.GetType().GetDefaultValue();

            // Assert
            defaultValue.Should().Be(default(Person));
        }

        #endregion

        #region IsNullable Tests

        [Fact]
        public void ShouldBeNullableType()
        {
            // Arrange
            var nullableType = typeof(bool?);

            // Act
            var isNullable = nullableType.IsNullable();

            // Assert
            isNullable.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotBeNullableType()
        {
            // Arrange
            var valueType = typeof(bool);

            // Act
            var isNullable = valueType.IsNullable();

            // Assert
            isNullable.Should().BeFalse();
        }

        #endregion

        #region ToTypeName Tests

        [Fact]
        public void ShouldGetTypeNameOfValueType()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var typeName = type.GetFormattedName();

            // Assert
            typeName.Should().Be("String");
        }

        [Fact]
        public void ShouldGetTypeNameOfReferenceType()
        {
            // Arrange
            var type = typeof(Person);

            // Act
            var typeName = type.GetFormattedName();

            // Assert
            typeName.Should().Be("Person");
        }

        [Fact]
        public void ShouldGetTypeNameOfGenericType()
        {
            // Arrange
            var type = typeof(GenericClass1<string, int>);

            // Act
            var typeName = type.GetFormattedName();

            // Assert
            typeName.Should().Be("GenericClass1<String, Int32>");
        }

        [Fact]
        public void ShouldGetTypeNameOfNestedGenericType()
        {
            // Arrange
            var type = typeof(GenericClass1<GenericClass1<int, float>, Person>);

            // Act
            var typeName = type.GetFormattedName();

            // Assert
            typeName.Should().Be("GenericClass1<GenericClass1<Int32, Single>, Person>");
        }

        [Fact]
        public void ShouldGetTypeNameOfNullableType()
        {
            // Arrange
            var type = typeof(bool?);

            // Act
            var typeName = type.GetFormattedName();

            // Assert
            typeName.Should().Be("Nullable<Boolean>");
        }

        #endregion
    }
}