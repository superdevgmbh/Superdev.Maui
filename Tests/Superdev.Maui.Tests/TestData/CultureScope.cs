using System.Globalization;

namespace Superdev.Maui.Tests.TestData
{
    /// <summary>
    /// Switches the current thread's culture for the duration of a test and restores it afterwards.
    /// </summary>
    public sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo previousCulture;
        private readonly CultureInfo previousUiCulture;

        public CultureScope(string cultureName)
        {
            this.previousCulture = CultureInfo.CurrentCulture;
            this.previousUiCulture = CultureInfo.CurrentUICulture;

            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = this.previousCulture;
            CultureInfo.CurrentUICulture = this.previousUiCulture;
        }
    }
}
