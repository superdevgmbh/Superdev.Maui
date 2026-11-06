using System.Collections.ObjectModel;

namespace SuperdevMauiDemoApp.Services
{
    /// <summary>
    /// Collects navigation lifecycle entries across pages. Registered as a singleton, so that entries
    /// written by a page survive that page - which is what makes the disposal of a replaced page observable.
    /// </summary>
    public class NavigationDebugLog
    {
        public ObservableCollection<string> Entries { get; } = new ObservableCollection<string>();

        public void Add(string source, string entry)
        {
            this.Entries.Insert(0, $"{DateTime.Now:HH:mm:ss.fff}  {source}: {entry}");
        }

        public void Clear()
        {
            this.Entries.Clear();
        }
    }
}
