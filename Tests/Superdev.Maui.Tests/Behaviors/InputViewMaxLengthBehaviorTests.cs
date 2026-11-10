using Superdev.Maui.Behaviors;

namespace Superdev.Maui.Tests.Behaviors
{
    public class InputViewMaxLengthBehaviorTests
    {
        [Fact]
        public void TextChanged_EntryTextLongerThanMaxLength_TruncatesText()
        {
            // Arrange
            var entry = new Entry();
            entry.Behaviors.Add(new InputViewMaxLengthBehavior { MaxLength = 3 });

            // Act
            entry.Text = "abcdef";

            // Assert
            entry.Text.Should().Be("abc");
        }

        [Fact]
        public void TextChanged_EditorTextLongerThanMaxLength_TruncatesText()
        {
            // Arrange
            var editor = new Editor();
            editor.Behaviors.Add(new InputViewMaxLengthBehavior { MaxLength = 3 });

            // Act
            editor.Text = "abcdef";

            // Assert
            editor.Text.Should().Be("abc");
        }

        [Fact]
        public void TextChanged_TextWithinMaxLength_KeepsText()
        {
            // Arrange
            var entry = new Entry();
            entry.Behaviors.Add(new InputViewMaxLengthBehavior { MaxLength = 3 });

            // Act
            entry.Text = "ab";

            // Assert
            entry.Text.Should().Be("ab");
        }

        [Fact]
        public void OnAttachedTo_ElementWithoutText_ThrowsInvalidOperationException()
        {
            // Arrange
            var label = new Label();

            // Act
            var act = () => label.Behaviors.Add(new InputViewMaxLengthBehavior());

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
