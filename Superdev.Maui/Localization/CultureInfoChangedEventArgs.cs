using System.Globalization;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Localization
{
    [Preserve(AllMembers = true)]
    public class LanguageChangedEventArgs : EventArgs
    {
        public LanguageChangedEventArgs(CultureInfo cultureInfo)
        {
            this.CultureInfo = cultureInfo;
        }

        public CultureInfo CultureInfo { get; private set; }
    }
}