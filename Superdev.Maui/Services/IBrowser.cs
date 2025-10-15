namespace Superdev.Maui.Services
{
    public interface IBrowser
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IBrowser"/>.
        /// </summary>
        public static IBrowser Current { get; } = Browser.Current;

        Task<bool> TryOpenAsync(string uri);

        Task<bool> TryOpenAsync(Uri uri);

        Task<bool> TryOpenAsync(string uri, BrowserLaunchOptions options);

        Task<bool> TryOpenAsync(Uri uri, BrowserLaunchOptions options);
    }
}