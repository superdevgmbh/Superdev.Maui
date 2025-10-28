namespace Superdev.Maui.Navigation
{
    public interface IPageResolver
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IPageResolver"/>.
        /// </summary>
        public static IPageResolver Current => PageResolver.Current;

        Page ResolvePage(string pageName);

        TBindableObject ResolvePage<TBindableObject>(string pageName) where TBindableObject : BindableObject;
    }
}