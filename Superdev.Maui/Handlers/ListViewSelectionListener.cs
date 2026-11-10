using System.ComponentModel;
using System.Runtime.CompilerServices;

// ListView is deprecated in MAUI 10, but CustomViewCell only lives in one, and its handlers need this listener.
#pragma warning disable CS0618

namespace Superdev.Maui.Handlers
{
    /// <summary>
    /// Tells the rows of a <see cref="ListView"/> when its selection changes, which the list does not tell its cells.
    /// </summary>
    /// <remarks>
    /// The rows are held weakly, because a list outlives the rows it discards.
    /// </remarks>
    internal sealed class ListViewSelectionListener
    {
        private static readonly ConditionalWeakTable<ListView, ListViewSelectionListener> Listeners = new ConditionalWeakTable<ListView, ListViewSelectionListener>();

        private readonly ConditionalWeakTable<object, Action> rows = new ConditionalWeakTable<object, Action>();

        private ListViewSelectionListener(ListView listView)
        {
            listView.PropertyChanged += this.OnListViewPropertyChanged;
        }

        /// <summary>
        /// Calls <paramref name="onSelectionChanged"/> whenever <paramref name="listView"/>'s selection changes, for as
        /// long as <paramref name="row"/> is alive.
        /// </summary>
        internal static void Register(ListView listView, object row, Action onSelectionChanged)
        {
            Listeners.GetValue(listView, l => new ListViewSelectionListener(l)).rows.AddOrUpdate(row, onSelectionChanged);
        }

        private void OnListViewPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ListView.SelectedItemProperty.PropertyName)
            {
                foreach (var (_, onSelectionChanged) in this.rows)
                {
                    onSelectionChanged();
                }
            }
        }
    }
}
