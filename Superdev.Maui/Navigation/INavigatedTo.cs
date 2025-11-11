namespace Superdev.Maui.Navigation
{
    public interface INavigatedTo
    {
        Task NavigatedToAsync();
    }

    public interface INavigatedTo<in T>
    {
        Task NavigatedToAsync(T parameter);
    }
}