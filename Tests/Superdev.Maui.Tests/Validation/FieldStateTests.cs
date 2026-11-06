using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class FieldStateTests
    {
        [Fact]
        public void ShouldStartClean()
        {
            var field = new FieldState("UserName");

            field.PropertyName.Should().Be("UserName");
            field.Errors.Should().BeEmpty();
            field.Messages.Should().BeEmpty();
            field.FirstMessage.Should().BeNull();
            field.HasErrors.Should().BeFalse();
            field.IsTouched.Should().BeFalse();
            field.IsDirty.Should().BeFalse();
            field.IsPending.Should().BeFalse();
        }

        [Fact]
        public void ShouldNotify_WhenErrorsAreSet()
        {
            // Arrange
            var field = new FieldState("UserName");
            var raised = new List<string?>();
            field.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            field.SetErrors(new[] { new ValidationError("UserName", "NotEmpty", "required") });

            // Assert
            field.Messages.Should().Equal("required");
            field.FirstMessage.Should().Be("required");
            field.HasErrors.Should().BeTrue();
            raised.Should().Contain(new[] { "Errors", "Messages", "FirstMessage", "HasErrors" });
        }

        [Fact]
        public void ShouldNotRaiseFirstMessageOrHasErrors_WhenTheyDidNotChange()
        {
            // Arrange
            var field = new FieldState("UserName");
            field.SetErrors(new[] { new ValidationError("UserName", "NotEmpty", "required") });
            var raised = new List<string?>();
            field.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act - same first message, still errors
            field.SetErrors(new[] { new ValidationError("UserName", "NotEmpty", "required"), new ValidationError("UserName", "Custom", "taken") });

            // Assert
            raised.Should().NotContain("FirstMessage");
            raised.Should().NotContain("HasErrors");
            raised.Should().Contain("Messages");
        }

        [Fact]
        public void ShouldNotifyTouchedDirtyPending_OnlyWhenTheyChange()
        {
            // Arrange
            var field = new FieldState("UserName");
            var raised = new List<string?>();
            field.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            field.SetTouched(true);
            field.SetTouched(true);
            field.SetDirty(true);
            field.SetPending(true);
            field.SetPending(false);

            // Assert
            raised.Should().Equal("IsTouched", "IsDirty", "IsPending", "IsPending");
        }
    }
}
