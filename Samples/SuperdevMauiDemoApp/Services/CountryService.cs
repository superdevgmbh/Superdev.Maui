using SuperdevMauiDemoApp.Model;

namespace SuperdevMauiDemoApp.Services
{
    public class CountryService : ICountryService
    {
        public Task<IEnumerable<CountryDto>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<CountryDto>>(new List<CountryDto>
            {
                new CountryDto{ Id = 1, Name = "Switzerland"},
                new CountryDto{ Id = 2, Name = "Germany"},
                new CountryDto{ Id = 3, Name = "France"},
                new CountryDto{ Id = 4, Name = "French Guiana"},
                new CountryDto{ Id = 5, Name = "French Guiana 1"},
                new CountryDto{ Id = 6, Name = "French Guiana 2"},
                new CountryDto{ Id = 7, Name = "French Polynesia"},
                new CountryDto{ Id = 8, Name = "French Southern Territories (the)"},
                new CountryDto{ Id = 9, Name = "USA"}
            });
        }
    }
}