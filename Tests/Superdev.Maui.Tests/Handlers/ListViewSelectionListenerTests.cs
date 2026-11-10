using System.Runtime.CompilerServices;
using Superdev.Maui.Handlers;
using Superdev.Maui.Tests.Navigation.TestDoubles;

// ListView is deprecated in MAUI 10, but the listener exists for the rows of one.
#pragma warning disable CS0618

namespace Superdev.Maui.Tests.Handlers
{
    public class ListViewSelectionListenerTests
    {
        public ListViewSelectionListenerTests()
        {
            // A ListView needs a dispatcher when it is created.
            TestDispatcherProvider.EnsureInstalled();
        }

        [Fact]
        public void Register_SelectedItemChanges_CallsTheRow()
        {
            // Arrange
            var listView = new ListView { ItemsSource = new[] { "a", "b" } };
            var calls = 0;
            ListViewSelectionListener.Register(listView, new object(), () => calls++);

            // Act
            listView.SelectedItem = "a";

            // Assert
            calls.Should().Be(1);
        }

        [Fact]
        public void Register_OtherPropertyChanges_DoesNotCallTheRow()
        {
            // Arrange
            var listView = new ListView();
            var calls = 0;
            ListViewSelectionListener.Register(listView, new object(), () => calls++);

            // Act
            listView.HasUnevenRows = true;

            // Assert
            calls.Should().Be(0);
        }

        [Fact]
        public void Register_TwoRows_CallsBothRows()
        {
            // Arrange
            var listView = new ListView { ItemsSource = new[] { "a", "b" } };
            var firstRowCalls = 0;
            var secondRowCalls = 0;
            ListViewSelectionListener.Register(listView, new object(), () => firstRowCalls++);
            ListViewSelectionListener.Register(listView, new object(), () => secondRowCalls++);

            // Act
            listView.SelectedItem = "b";

            // Assert
            firstRowCalls.Should().Be(1);
            secondRowCalls.Should().Be(1);
        }

        [Fact]
        public void Register_SameRowTwice_CallsOnlyTheLatestCallback()
        {
            // Arrange
            var listView = new ListView { ItemsSource = new[] { "a", "b" } };
            var row = new object();
            var firstCalls = 0;
            var latestCalls = 0;
            ListViewSelectionListener.Register(listView, row, () => firstCalls++);
            ListViewSelectionListener.Register(listView, row, () => latestCalls++);

            // Act
            listView.SelectedItem = "a";

            // Assert
            firstCalls.Should().Be(0);
            latestCalls.Should().Be(1);
        }

        [Fact]
        public void Register_RowOfAnotherListView_IsNotCalled()
        {
            // Arrange
            var listView = new ListView { ItemsSource = new[] { "a", "b" } };
            var otherListView = new ListView { ItemsSource = new[] { "a", "b" } };
            var calls = 0;
            ListViewSelectionListener.Register(otherListView, new object(), () => calls++);

            // Act
            listView.SelectedItem = "a";

            // Assert
            calls.Should().Be(0);
        }

        [Fact]
        public void Register_RowCollected_IsNotCalledAnyMore()
        {
            // Arrange
            var listView = new ListView { ItemsSource = new[] { "a", "b" } };
            var calls = new StrongBox<int>();
            RegisterRowWithoutReference(listView, calls);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Act
            listView.SelectedItem = "a";

            // Assert
            calls.Value.Should().Be(0, "the list holds its rows weakly");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RegisterRowWithoutReference(ListView listView, StrongBox<int> calls)
        {
            ListViewSelectionListener.Register(listView, new object(), () => calls.Value++);
        }
    }
}
