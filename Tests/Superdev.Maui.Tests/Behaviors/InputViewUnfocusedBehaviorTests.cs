using Superdev.Maui.Behaviors;

namespace Superdev.Maui.Tests.Behaviors
{
    public class InputViewUnfocusedBehaviorTests
    {
        [Theory]
        [InlineData(TextDecorationFlags.Trim, "  Lake   Zurich  ", "Lake Zurich")]
        [InlineData(TextDecorationFlags.TrimStart, "  Lake   Zurich  ", "Lake   Zurich  ")]
        [InlineData(TextDecorationFlags.TrimEnd, "  Lake   Zurich  ", "  Lake   Zurich")]
        [InlineData(TextDecorationFlags.TrimStartEnd, "  Lake   Zurich  ", "Lake   Zurich")]
        [InlineData(TextDecorationFlags.None, "  Lake   Zurich  ", "  Lake   Zurich  ")]
        public void Unfocused_WithDecorationFlags_DecoratesText(TextDecorationFlags decorationFlags, string text, string expectedText)
        {
            // Arrange
            var entry = new Entry { Text = text };
            entry.Behaviors.Add(new InputViewUnfocusedBehavior { DecorationFlags = decorationFlags });
            IView view = entry;
            view.IsFocused = true;

            // Act
            view.IsFocused = false;

            // Assert
            entry.Text.Should().Be(expectedText);
        }

        [Fact]
        public void OnAttachedTo_ElementWithoutText_ThrowsInvalidOperationException()
        {
            // Arrange
            var label = new Label();

            // Act
            var act = () => label.Behaviors.Add(new InputViewUnfocusedBehavior());

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
