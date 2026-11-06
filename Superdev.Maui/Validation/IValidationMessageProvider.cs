namespace Superdev.Maui.Validation
{
    /// <summary>
    /// Resolves an error code to the message the user sees. The arguments are what the rule knows —
    /// <c>PropertyName</c> always, plus the rule's own, such as <c>MinimumLength</c> — and a message may
    /// reference them as <c>{PropertyName}</c>, <c>{MinimumLength}</c>.
    /// </summary>
    public interface IValidationMessageProvider
    {
        string GetMessage(string errorCode, IReadOnlyDictionary<string, object?> arguments);

        /// <summary>
        /// The name <paramref name="propertyName"/> has in messages, or <c>null</c> when the provider has none — the
        /// property name split into words is used then. A property's <c>WithDisplayName</c> takes precedence.
        /// </summary>
        string? GetDisplayName(string propertyName) => null;
    }
}
