using System.Collections.ObjectModel;

namespace Superdev.Maui.Validation
{
    /// <summary>
    /// A single validation failure: which property, which rule (<see cref="ErrorCode"/>), the message the user
    /// sees, and the arguments the message template was filled with. This is deliberately the same shape a
    /// backend validation error has, so that results can be converted in either direction without thinking.
    /// </summary>
    public sealed record ValidationError(
        string PropertyName,
        string ErrorCode,
        string Message,
        IReadOnlyDictionary<string, object?> Arguments)
    {
        public static readonly IReadOnlyDictionary<string, object?> EmptyArguments =
            new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(0));

        public ValidationError(string propertyName, string errorCode, string message)
            : this(propertyName, errorCode, message, EmptyArguments)
        {
        }
    }
}
