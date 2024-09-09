using SuperdevMauiDemoApp.Model;

namespace SuperdevMauiDemoApp.Services
{
    public interface ICountryService
    {
        Task<IEnumerable<CountryDto>> GetAllAsync();
    }
}