using SuperdevMauiDemoApp.Model;

namespace SuperdevMauiDemoApp.Services.Validation
{
    public interface IValidationService
    {
        Task<ValidationResultDto> ValidatePersonAsync(PersonDto personDto);
    }
}