namespace SuperdevMauiDemoApp.Services.Validation
{
    /// <summary>
    /// The answer <see cref="IValidationDemoService.SavePersonAsync"/> gives.
    /// </summary>
    public enum BackendOutcome
    {
        Success,
        FieldErrors,
        Throws,
    }
}
