using Superdev.Maui.Validation;
using SuperdevMauiDemoApp.Model;

namespace SuperdevMauiDemoApp.Services.Validation
{
    public class ValidationDemoService : IValidationDemoService
    {
        private const int MinimumUserNameLength = 3;

        private static readonly string[] TakenUserNames = { "thomasgalliker", "superdev", "admin" };

        public BackendOutcome Outcome { get; set; } = BackendOutcome.FieldErrors;

        public async Task<ValidationResult> SavePersonAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(600), cancellationToken);

            switch (this.Outcome)
            {
                case BackendOutcome.Success:
                    return ValidationResult.Success;

                case BackendOutcome.Throws:
                    throw new HttpRequestException("The server is not reachable.");

                default:
                    // The shape every backend answers with: property → messages. One flat key, one nested key for the
                    // included address, one Identity-style code the Email field claims by alias, and one key nobody
                    // owns, which must show up under Errors.Form rather than disappear.
                    return ValidationResult.FromErrors(new Dictionary<string, string[]>
                    {
                        ["UserName"] = new[] { "This user name is reserved." },
                        ["Address.Street"] = new[] { "The street is not known at this postal code." },
                        ["DuplicateEmail"] = new[] { "An account with this e-mail address already exists." },
                        ["InvalidToken"] = new[] { "Your session has expired, please sign in again." },
                    });
            }
        }

        public async Task<ValidationResult> ValidatePersonAsync(PersonDto personDto, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);

            if (personDto.UserName == null || personDto.UserName.Length < MinimumUserNameLength)
            {
                return ValidationResult.FromErrors(new Dictionary<string, string[]>
                {
                    ["UserName"] = new[] { "Username must be at least three characters, otherwise we can't accept it. What about using 'lorem ipsum'?" },
                });
            }

            if (!IsUserNameAvailable(personDto.UserName))
            {
                return ValidationResult.FromErrors(new Dictionary<string, string[]>
                {
                    ["UserName"] = new[] { "User already exists!" },
                });
            }

            return ValidationResult.Success;
        }

        public async Task<bool> IsUserNameAvailableAsync(string? userName, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(800), cancellationToken);
            return IsUserNameAvailable(userName);
        }

        private static bool IsUserNameAvailable(string? userName)
        {
            return userName == null || !TakenUserNames.Contains(userName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
