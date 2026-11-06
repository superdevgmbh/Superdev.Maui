namespace Superdev.Maui.Validation
{
    /// <summary>
    /// One rule in a property's chain: a predicate — synchronous, or asynchronous with an optional debounce —
    /// the error code it reports under, the groups it belongs to and an optional message of its own.
    /// </summary>
    internal interface IPropertyRule
    {
        bool IsAsync { get; }

        bool IsActive(IReadOnlySet<string> activeGroups);
    }

    internal sealed class PropertyRule<TProperty> : IPropertyRule
    {
        public PropertyRule(string errorCode)
        {
            this.ErrorCode = errorCode;
        }

        public string ErrorCode { get; set; }

        public Func<TProperty, bool>? Predicate { get; init; }

        public Func<TProperty, CancellationToken, ValueTask<bool>>? AsyncPredicate { get; init; }

        public TimeSpan Debounce { get; init; }

        public Func<TProperty, IReadOnlyDictionary<string, object?>>? Arguments { get; init; }

        /// <summary>
        /// The property a cross-field rule compares with. Its display name is handed to the message as
        /// <c>{OtherPropertyName}</c>, resolved when the rule fails.
        /// </summary>
        public string? OtherPropertyName { get; init; }

        public Func<TProperty, string>? MessageOverride { get; set; }

        public HashSet<string> Groups { get; } = new HashSet<string>(StringComparer.Ordinal);

        public bool IsAsync => this.AsyncPredicate != null;

        /// <summary>
        /// A rule without a group always runs; a grouped rule runs when any of its groups is active.
        /// </summary>
        public bool IsActive(IReadOnlySet<string> activeGroups)
        {
            return this.Groups.Count == 0 || this.Groups.Overlaps(activeGroups);
        }
    }
}
