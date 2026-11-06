using System.Globalization;
using Superdev.Maui.Behaviors;

namespace Superdev.Maui.Tests.Behaviors
{
    public class ItemTappedBehaviorTests
    {
        [Fact]
        public void OnAttachedTo_MultipleSelection_ForcesSingleSelection()
        {
            // Arrange
            var collectionView = new CollectionView { SelectionMode = SelectionMode.Multiple };

            // Act
            collectionView.Behaviors.Add(new ItemTappedBehavior());

            // Assert
            collectionView.SelectionMode.Should().Be(SelectionMode.Single);
        }

        [Fact]
        public void SelectionChanged_ItemSelected_ExecutesCommandWithItem()
        {
            // Arrange
            var parameters = new List<object?>();
            var collectionView = CreateCollectionView(new ItemTappedBehavior { Command = new Command<object?>(parameters.Add) });

            // Act
            collectionView.SelectedItem = "b";

            // Assert
            parameters.Should().Equal("b");
        }

        [Fact]
        public void SelectionChanged_ItemSelected_ClearsSelection()
        {
            // Arrange
            var collectionView = CreateCollectionView(new ItemTappedBehavior { Command = new Command(() => { }) });

            // Act
            collectionView.SelectedItem = "b";

            // Assert
            collectionView.SelectedItem.Should().BeNull("the same item can be tapped again");
        }

        [Fact]
        public void SelectionChanged_WithCommandParameterAndConverter_ExecutesCommandWithConvertedParameter()
        {
            // Arrange
            var parameters = new List<object?>();
            var behavior = new ItemTappedBehavior
            {
                Command = new Command<object?>(parameters.Add),
                CommandParameter = "fixed",
                Converter = new SuffixConverter("!"),
            };
            var collectionView = CreateCollectionView(behavior);

            // Act
            collectionView.SelectedItem = "b";

            // Assert
            parameters.Should().Equal("fixed!");
        }

        [Fact]
        public void SelectionChanged_WithoutBindingContext_DoesNotExecuteCommand()
        {
            // Arrange
            var parameters = new List<object?>();
            var collectionView = CreateCollectionView(new ItemTappedBehavior { Command = new Command<object?>(parameters.Add) });
            collectionView.BindingContext = null;

            // Act
            collectionView.SelectedItem = "b";

            // Assert
            parameters.Should().BeEmpty();
        }

        [Fact]
        public void SelectionChanged_CommandCannotExecute_DoesNotExecuteCommand()
        {
            // Arrange
            var parameters = new List<object?>();
            var collectionView = CreateCollectionView(new ItemTappedBehavior { Command = new Command<object?>(parameters.Add, _ => false) });

            // Act
            collectionView.SelectedItem = "b";

            // Assert
            parameters.Should().BeEmpty();
        }

        private static CollectionView CreateCollectionView(ItemTappedBehavior behavior)
        {
            var collectionView = new CollectionView
            {
                BindingContext = new object(),
                ItemsSource = new[] { "a", "b" },
            };
            collectionView.Behaviors.Add(behavior);
            return collectionView;
        }

        private sealed class SuffixConverter : IValueConverter
        {
            private readonly string suffix;

            public SuffixConverter(string suffix)
            {
                this.suffix = suffix;
            }

            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                return $"{value}{this.suffix}";
            }

            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}
