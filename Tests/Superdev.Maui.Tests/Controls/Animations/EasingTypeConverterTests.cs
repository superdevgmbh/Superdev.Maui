using System.Globalization;
using Superdev.Maui.Controls.Animations;

namespace Superdev.Maui.Tests.Controls.Animations
{
    public class EasingTypeConverterTests
    {
        private readonly EasingTypeConverter converter = new EasingTypeConverter();

        public static TheoryData<string, Easing> NamedEasings => new TheoryData<string, Easing>
        {
            { "Linear", Easing.Linear },
            { "SinOut", Easing.SinOut },
            { "SinIn", Easing.SinIn },
            { "SinInOut", Easing.SinInOut },
            { "CubicIn", Easing.CubicIn },
            { "CubicOut", Easing.CubicOut },
            { "CubicInOut", Easing.CubicInOut },
            { "BounceOut", Easing.BounceOut },
            { "BounceIn", Easing.BounceIn },
            { "SpringIn", Easing.SpringIn },
            { "SpringOut", Easing.SpringOut },
        };

        [Theory]
        [MemberData(nameof(NamedEasings))]
        public void ShouldConvertFromName(string name, Easing expectedEasing)
        {
            // Act
            var easing = this.converter.ConvertFromInvariantString(name);

            // Assert
            easing.Should().BeSameAs(expectedEasing);
        }

        [Theory]
        [InlineData("sinout")]
        [InlineData("SINOUT")]
        [InlineData("Easing.SinOut")]
        [InlineData("easing.sinout")]
        [InlineData("  SinOut  ")]
        public void ShouldConvertFromName_IgnoringCaseAndPrefix(string name)
        {
            // Act
            var easing = this.converter.ConvertFromInvariantString(name);

            // Assert
            easing.Should().BeSameAs(Easing.SinOut);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Nope")]
        [InlineData("Easing.")]
        public void ShouldThrow_WhenNameIsUnknown(string name)
        {
            // Act
            Action action = () => this.converter.ConvertFromInvariantString(name);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*Valid values are: Linear, SinOut, *");
        }

        [Fact]
        public void ShouldThrow_WhenValueIsNotAString()
        {
            // Act
            Action action = () => this.converter.ConvertFrom(null, CultureInfo.InvariantCulture, 42);

            // Assert
            action.Should().Throw<NotSupportedException>();
        }

        [Theory]
        [MemberData(nameof(NamedEasings))]
        public void ShouldConvertToName(string expectedName, Easing easing)
        {
            // Act
            var name = this.converter.ConvertToInvariantString(easing);

            // Assert
            name.Should().Be(expectedName);
        }

        [Fact]
        public void ShouldThrow_WhenConvertingAnUnnamedEasingToString()
        {
            // Arrange
            var customEasing = new Easing(t => t * t);

            // Act
            Action action = () => this.converter.ConvertToInvariantString(customEasing);

            // Assert
            action.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void ShouldOnlyConvertFromAndToString()
        {
            // Assert
            this.converter.CanConvertFrom(typeof(string)).Should().BeTrue();
            this.converter.CanConvertFrom(typeof(int)).Should().BeFalse();
            this.converter.CanConvertTo(typeof(string)).Should().BeTrue();
            this.converter.CanConvertTo(typeof(Easing)).Should().BeFalse();
        }
    }
}
