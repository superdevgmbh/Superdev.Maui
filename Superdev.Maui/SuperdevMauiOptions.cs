namespace Superdev.Maui
{
    public class SuperdevMauiOptions
    {
        /// <summary>
        /// Sets <see cref="Layout.IgnoreSafeArea" /> to <c>true</c> for all layouts.
        /// If you ignore safe area for all layouts, it's your responsibility to add safe area padding if necessary.
        /// This is the workaround discussed in this MAUI issue: https://github.com/dotnet/maui/issues/12417.
        /// Default: <c>false</c>
        /// </summary>
        public bool IgnoreSafeArea { get; set; } = false;

        /// <summary>
        /// When enabled, automatically clears Behaviors, Triggers, and Effects
        /// from all UI elements once a page is no longer in use.
        /// </summary>
        public bool AutoCleanupPage { get; set; } = true;
    }
}