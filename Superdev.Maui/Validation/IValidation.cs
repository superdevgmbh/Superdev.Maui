namespace Superdev.Maui.Validation
{
    public interface IValidation
    {
        string[] PropertyNames { get; }

        Task<Dictionary<string, List<string>>> GetErrors();
    }
}