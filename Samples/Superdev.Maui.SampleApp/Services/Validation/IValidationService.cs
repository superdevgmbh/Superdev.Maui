using Superdev.Maui.SampleApp.Model;

namespace Superdev.Maui.SampleApp.Services.Validation
{
    public interface IValidationService
    {
        Task<ValidationResultDto> ValidatePersonAsync(PersonDto personDto);
    }
}