using System.Globalization;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Localization
{
    [Preserve(AllMembers = true)]
    public class LanguageChangingEventArgs : EventArgs
    {
        public LanguageChangingEventArgs(CultureInfo cultureInfo)
        {
            this.CultureInfo = cultureInfo;
        }

        public CultureInfo CultureInfo { get; }
    }
}