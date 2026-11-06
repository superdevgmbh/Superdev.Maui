using Superdev.Maui.Validation;
using SuperdevMauiDemoApp.Model;

namespace SuperdevMauiDemoApp.Services.Validation
{
    /// <summary>
    /// Stands in for the server: answers a save with whatever <see cref="Outcome"/> is selected, judges a person, and
    /// checks a user name for availability with a little latency, so that the demos can show every path of the
    /// validation loop.
    /// </summary>
    public interface IValidationDemoService
    {
        BackendOutcome Outcome { get; set; }

        Task<ValidationResult> SavePersonAsync(CancellationToken cancellationToken);

        Task<ValidationResult> ValidatePersonAsync(PersonDto personDto, CancellationToken cancellationToken);

        Task<bool> IsUserNameAvailableAsync(string? userName, CancellationToken cancellationToken);
    }
}
