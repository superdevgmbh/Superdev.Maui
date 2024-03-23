using Superdev.Maui.SampleApp.Model;

namespace Superdev.Maui.SampleApp.Services
{
    public interface ICountryService
    {
        Task<IEnumerable<CountryDto>> GetAllAsync();
    }
}