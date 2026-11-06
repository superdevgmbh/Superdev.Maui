using Superdev.Maui.Behaviors;

namespace Superdev.Maui.Tests.Behaviors
{
    public class BehaviorBaseTests
    {
        [Fact]
        public void OnAttachedTo_ElementWithBindingContext_TakesOverElementAndBindingContext()
        {
            // Arrange
            var bindingContext = new object();
            var label = new Label { BindingContext = bindingContext };
            var behavior = new BehaviorBase<Label>();

            // Act
            label.Behaviors.Add(behavior);

            // Assert
            behavior.AssociatedObject.Should().BeSameAs(label);
            behavior.BindingContext.Should().BeSameAs(bindingContext);
        }

        [Fact]
        public void BindingContextChanged_ToNullAndBack_StaysAttached()
        {
            // Arrange
            var label = new Label { BindingContext = new object() };
            var behavior = new BehaviorBase<Label>();
            label.Behaviors.Add(behavior);
            var nextBindingContext = new object();

            // Act - what a recycled cell does
            label.BindingContext = null;
            label.BindingContext = nextBindingContext;

            // Assert
            behavior.AssociatedObject.Should().BeSameAs(label);
            behavior.BindingContext.Should().BeSameAs(nextBindingContext);
        }

        [Fact]
        public void OnDetachingFrom_Element_ClearsAssociatedObject()
        {
            // Arrange
            var label = new Label();
            var behavior = new BehaviorBase<Label>();
            label.Behaviors.Add(behavior);

            // Act
            label.Behaviors.Remove(behavior);

            // Assert
            behavior.AssociatedObject.Should().BeNull();
        }
    }
}
