using System.Globalization;
using System.Runtime.CompilerServices;

namespace Superdev.Maui.Tests
{
    /// <summary>
    /// Pins the culture of the test run, so that a test which asserts on formatted output does not
    /// depend on the culture of the machine it runs on.
    /// </summary>
    /// <remarks>
    /// Tests which verify that production code is culture-independent must set the culture themselves;
    /// see <see cref="CultureScope"/>.
    /// </remarks>
    internal static class TestCulture
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        }
    }

    /// <summary>
    /// Runs a block of code under a given culture and restores the previous one afterwards.
    /// </summary>
    internal sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo previousCulture;

        public CultureScope(string cultureName)
        {
            this.previousCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = this.previousCulture;
        }
    }
}
