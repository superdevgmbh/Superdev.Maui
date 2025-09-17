namespace Superdev.Maui.Services
{
    public interface IPageResolver
    {
        Page ResolvePage(string pageName);

        TBindableObject ResolvePage<TBindableObject>(string pageName) where TBindableObject : BindableObject;
    }
}