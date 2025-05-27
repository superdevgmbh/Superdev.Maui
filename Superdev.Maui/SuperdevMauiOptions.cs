namespace Superdev.Maui
{
    public class SuperdevMauiOptions
    {
        /// <summary>
        /// Sets <see cref="Layout.IgnoreSafeArea" /> to <c>true</c> for all layouts.
        /// This is the workaround discussed in this MAUI issue: https://github.com/dotnet/maui/issues/12417.
        /// </summary>
        public bool IgnoreSafeArea { get; set; }
    }
}