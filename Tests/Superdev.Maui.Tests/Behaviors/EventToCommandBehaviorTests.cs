using Superdev.Maui.Behaviors;

namespace Superdev.Maui.Tests.Behaviors
{
    public class EventToCommandBehaviorTests
    {
        [Fact]
        public void Event_Raised_ExecutesCommandWithEventArgs()
        {
            // Arrange
            var parameters = new List<object?>();
            var entry = new Entry { BindingContext = new object() };
            entry.Behaviors.Add(new EventToCommandBehavior
            {
                EventName = nameof(Entry.Completed),
                Command = new Command<object?>(parameters.Add),
            });

            // Act
            entry.SendCompleted();

            // Assert
            parameters.Should().ContainSingle().Which.Should().BeOfType<EventArgs>();
        }

        [Fact]
        public void Event_WithEventArgsParameterPath_ExecutesCommandWithPropertyOfEventArgs()
        {
            // Arrange
            var parameters = new List<object?>();
            var entry = new Entry { BindingContext = new object() };
            entry.Behaviors.Add(new EventToCommandBehavior
            {
                EventName = nameof(Entry.TextChanged),
                EventArgsParameterPath = nameof(TextChangedEventArgs.NewTextValue),
                Command = new Command<object?>(parameters.Add),
            });

            // Act
            entry.Text = "Lake Zurich";

            // Assert
            parameters.Should().Equal("Lake Zurich");
        }

        [Fact]
        public void Event_WithCommandParameter_ExecutesCommandWithCommandParameter()
        {
            // Arrange
            var parameters = new List<object?>();
            var entry = new Entry { BindingContext = new object() };
            entry.Behaviors.Add(new EventToCommandBehavior
            {
                EventName = nameof(Entry.Completed),
                Command = new Command<object?>(parameters.Add),
                CommandParameter = 42,
            });

            // Act
            entry.SendCompleted();

            // Assert
            parameters.Should().Equal(42);
        }

        [Fact]
        public void Event_WithoutBindingContext_DoesNotExecuteCommand()
        {
            // Arrange
            var parameters = new List<object?>();
            var entry = new Entry();
            entry.Behaviors.Add(new EventToCommandBehavior
            {
                EventName = nameof(Entry.Completed),
                Command = new Command<object?>(parameters.Add),
            });

            // Act
            entry.SendCompleted();

            // Assert
            parameters.Should().BeEmpty();
        }

        [Fact]
        public void EventName_ChangedWhileAttached_ListensToTheNewEventOnly()
        {
            // Arrange
            var parameters = new List<object?>();
            var entry = new Entry { BindingContext = new object() };
            var behavior = new EventToCommandBehavior
            {
                EventName = nameof(Entry.Completed),
                Command = new Command<object?>(parameters.Add),
            };
            entry.Behaviors.Add(behavior);

            // Act
            behavior.EventName = nameof(Entry.TextChanged);
            entry.SendCompleted();
            entry.Text = "a";

            // Assert
            parameters.Should().ContainSingle().Which.Should().BeOfType<TextChangedEventArgs>();
        }

        [Fact]
        public void OnAttachedTo_UnknownEventName_ThrowsArgumentException()
        {
            // Arrange
            var entry = new Entry();
            var behavior = new EventToCommandBehavior { EventName = "NoSuchEvent" };

            // Act
            var act = () => entry.Behaviors.Add(behavior);

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}
