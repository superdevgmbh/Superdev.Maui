using Microsoft.Maui.Controls.Internals;
using Superdev.Maui.Behaviors;

namespace Superdev.Maui.Tests.Behaviors
{
    public class InputViewCompletedBehaviorTests
    {
        [Fact]
        public void Completed_WithTargetElement_RequestsFocusOnTarget()
        {
            // Arrange
            var targetHandlerMock = new Mock<IViewHandler>();
            targetHandlerMock.Setup(h => h.Invoke(nameof(IView.Focus), It.IsAny<object>()))
                .Callback<string, object?>((_, args) => (args as FocusRequest)?.TrySetResult(true));
            var target = new Entry { Handler = targetHandlerMock.Object };
            var entry = new Entry();
            entry.Behaviors.Add(new InputViewCompletedBehavior { TargetElement = target });

            // Act
            entry.SendCompleted();

            // Assert
            targetHandlerMock.Verify(h => h.Invoke(nameof(IView.Focus), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void Completed_WithTargetElementName_RequestsFocusOnNamedSibling()
        {
            // Arrange
            var targetHandlerMock = new Mock<IViewHandler>();
            targetHandlerMock.Setup(h => h.Invoke(nameof(IView.Focus), It.IsAny<object>()))
                .Callback<string, object?>((_, args) => (args as FocusRequest)?.TrySetResult(true));
            var target = new Entry { Handler = targetHandlerMock.Object };
            var entry = new Entry();
            entry.Behaviors.Add(new InputViewCompletedBehavior { TargetElementName = "Target" });
            var layout = new VerticalStackLayout { entry, target };
            INameScope nameScope = new NameScope();
            NameScope.SetNameScope(layout, nameScope);
            nameScope.RegisterName("Target", target);

            // Act
            entry.SendCompleted();

            // Assert
            targetHandlerMock.Verify(h => h.Invoke(nameof(IView.Focus), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void OnAttachedTo_ElementWhichIsNoEntryOrEditor_ThrowsInvalidOperationException()
        {
            // Arrange
            var label = new Label();

            // Act
            var act = () => label.Behaviors.Add(new InputViewCompletedBehavior());

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
