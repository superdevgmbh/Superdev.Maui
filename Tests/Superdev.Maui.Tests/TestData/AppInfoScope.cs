using System.Reflection;
using Microsoft.Maui.ApplicationModel;

namespace Superdev.Maui.Tests.TestData
{
    /// <summary>
    /// Replaces <see cref="AppInfo.Current"/> with a fake whose <see cref="IAppInfo.RequestedTheme"/> a test sets. MAUI reads
    /// the platform theme from there, so a test can change the system theme. MAUI offers no public setter for the
    /// implementation, so its internal one is called through reflection, and the previous implementation is restored on
    /// dispose. Tests using it belong to the <see cref="ApplicationCollection"/>, since the implementation is global.
    /// </summary>
    internal sealed class AppInfoScope : IDisposable
    {
        private static readonly MethodInfo SetCurrentMethod =
            typeof(AppInfo).GetMethod("SetCurrent", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("MAUI's AppInfo.SetCurrent was not found.");

        private readonly IAppInfo previousAppInfo;

        public AppInfoScope(AppTheme requestedTheme)
        {
            this.previousAppInfo = AppInfo.Current;
            this.RequestedTheme = requestedTheme;

            var appInfoMock = new Mock<IAppInfo>();
            appInfoMock.SetupGet(a => a.RequestedTheme).Returns(() => this.RequestedTheme);
            SetCurrentMethod.Invoke(null, [appInfoMock.Object]);
        }

        /// <summary>
        /// The system theme the fake reports.
        /// </summary>
        public AppTheme RequestedTheme { get; set; }

        public void Dispose()
        {
            SetCurrentMethod.Invoke(null, [this.previousAppInfo]);
        }
    }
}
