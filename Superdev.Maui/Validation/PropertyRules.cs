using System.Text.RegularExpressions;

namespace Superdev.Maui.Validation
{
    internal sealed class ValidationRunOptions
    {
        public required IReadOnlySet<string> ActiveGroups { get; init; }

        public required IValidationMessageProvider MessageProvider { get; init; }

        public required TimeProvider TimeProvider { get; init; }

        /// <summary>
        /// The name a property has in messages: its <c>WithDisplayName</c>, a translation, or its words.
        /// </summary>
        public required Func<string, string> DisplayNameOf { get; init; }

        /// <summary>
        /// An explicit validation runs asynchronous rules at once; a validation triggered by a change honours
        /// their debounce.
        /// </summary>
        public bool Immediate { get; init; }
    }

    /// <summary>
    /// What the synchronous rules of a chain found, and whether its asynchronous rules are still to run —
    /// which they are only when every synchronous rule passed.
    /// </summary>
    internal readonly record struct SyncOutcome(IReadOnlyList<(ValidationError Error, IPropertyRule Rule)> Errors, bool AsyncRulesPending)
    {
        public static readonly IReadOnlyList<(ValidationError Error, IPropertyRule Rule)> NoErrors =
            Array.Empty<(ValidationError, IPropertyRule)>();

        public static readonly SyncOutcome Clean = new SyncOutcome(NoErrors, false);

        public static readonly SyncOutcome CleanWithAsyncRules = new SyncOutcome(NoErrors, true);
    }

    /// <summary>
    /// The rule chain of one property: the accessor, the rules in declaration order, and everything declared
    /// for the property as a whole — conditions, aliases, dependencies, the display name.
    /// </summary>
    internal abstract partial class PropertyRules
    {
        [GeneratedRegex("(?<=[a-z0-9])([A-Z])")]
        private static partial Regex WordBoundaryRegex();

        protected PropertyRules(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        public string PropertyName { get; }

        public Func<string>? DisplayNameProvider { get; set; }

        public List<Func<bool>> Conditions { get; } = new List<Func<bool>>();

        public HashSet<string> Aliases { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public List<Func<string, bool>> AliasPredicates { get; } = new List<Func<string, bool>>();

        public HashSet<string> Dependencies { get; } = new HashSet<string>(StringComparer.Ordinal);

        public ValidationMode? ModeOverride { get; set; }

        public ValidationBehavior Behavior { get; set; } = ValidationBehavior.StopAtFirstError;

        /// <summary>
        /// Whether the conditions held when the chain was last looked at, <c>null</c> before that — so that a
        /// property change can tell that a condition has just turned on or off.
        /// </summary>
        public bool? LastApplies { get; set; }

        public bool HasConditions => this.Conditions.Count > 0;

        /// <summary>The groups of each rule, in declaration order.</summary>
        public abstract IEnumerable<IReadOnlySet<string>> RuleGroups();

        public abstract object? GetValue();

        public abstract bool Contains(IPropertyRule rule);

        /// <summary>
        /// Whether the active synchronous rules pass right now — the silent check behind <see cref="ViewModelValidation.IsValid"/>.
        /// Asynchronous rules are skipped; their last verdict is judged by the caller.
        /// </summary>
        public abstract bool PassesSynchronously(IReadOnlySet<string> activeGroups);

        /// <summary>Runs the active synchronous rules, and says whether asynchronous rules are left to run.</summary>
        public abstract SyncOutcome ValidateSync(ValidationRunOptions options);

        /// <summary>Runs the active asynchronous rules, each after its debounce unless the run is immediate.</summary>
        public abstract ValueTask<IReadOnlyList<(ValidationError Error, IPropertyRule Rule)>> ValidateAsyncRulesAsync(
            ValidationRunOptions options,
            CancellationToken cancellationToken);

        public bool MatchesAlias(string key)
        {
            return this.Aliases.Contains(key) || this.AliasPredicates.Any(p => p(key));
        }

        public bool AppliesNow()
        {
            return this.Conditions.All(condition => condition());
        }

        /// <summary>
        /// <see cref="AppliesNow"/>, or <c>null</c> when a condition throws — it reads state the view model has not
        /// loaded yet, say. For the silent looks at a condition; the property's own validation reports the exception.
        /// </summary>
        public bool? TryAppliesNow()
        {
            try
            {
                return this.AppliesNow();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// <c>UserName</c> → <c>User Name</c>: the display name in messages when nothing else names the property.
        /// </summary>
        internal static string Humanize(string propertyName)
        {
            return WordBoundaryRegex().Replace(propertyName, " $1");
        }
    }

    internal sealed class PropertyRules<TProperty> : PropertyRules
    {
        private readonly Func<TProperty> accessor;

        public PropertyRules(string propertyName, Func<TProperty> accessor)
            : base(propertyName)
        {
            this.accessor = accessor;
        }

        public List<PropertyRule<TProperty>> Rules { get; } = new List<PropertyRule<TProperty>>();

        public PropertyRule<TProperty>? LastRule => this.Rules.Count > 0 ? this.Rules[^1] : null;

        public override IEnumerable<IReadOnlySet<string>> RuleGroups()
        {
            return this.Rules.Select(rule => (IReadOnlySet<string>)rule.Groups);
        }

        public override object? GetValue()
        {
            return this.accessor();
        }

        public override bool Contains(IPropertyRule rule)
        {
            return rule is PropertyRule<TProperty> typed && this.Rules.Contains(typed);
        }

        public override bool PassesSynchronously(IReadOnlySet<string> activeGroups)
        {
            if (!this.AppliesNow())
            {
                return true;
            }

            var value = this.accessor();
            return this.Rules.All(rule => rule.IsAsync || !rule.IsActive(activeGroups) || rule.Predicate!(value));
        }

        public override SyncOutcome ValidateSync(ValidationRunOptions options)
        {
            var applies = this.AppliesNow();
            this.LastApplies = applies;

            if (!applies)
            {
                return SyncOutcome.Clean;
            }

            var value = this.accessor();
            List<(ValidationError Error, IPropertyRule Rule)>? errors = null;
            var asyncRulesPending = false;

            foreach (var rule in this.Rules)
            {
                if (!rule.IsActive(options.ActiveGroups))
                {
                    continue;
                }

                if (rule.IsAsync)
                {
                    asyncRulesPending = true;
                    continue;
                }

                if (rule.Predicate!(value))
                {
                    continue;
                }

                (errors ??= new List<(ValidationError, IPropertyRule)>()).Add((this.CreateError(rule, value, options), rule));

                if (this.Behavior == ValidationBehavior.StopAtFirstError)
                {
                    break;
                }
            }

            if (errors != null)
            {
                // Asynchronous rules only run once every synchronous rule passed.
                return new SyncOutcome(errors, false);
            }

            return asyncRulesPending ? SyncOutcome.CleanWithAsyncRules : SyncOutcome.Clean;
        }

        public override async ValueTask<IReadOnlyList<(ValidationError Error, IPropertyRule Rule)>> ValidateAsyncRulesAsync(
            ValidationRunOptions options,
            CancellationToken cancellationToken)
        {
            var value = this.accessor();
            List<(ValidationError Error, IPropertyRule Rule)>? errors = null;

            foreach (var rule in this.Rules)
            {
                if (!rule.IsAsync || !rule.IsActive(options.ActiveGroups))
                {
                    continue;
                }

                if (!options.Immediate && rule.Debounce > TimeSpan.Zero)
                {
                    await Task.Delay(rule.Debounce, options.TimeProvider, cancellationToken);
                }

                if (await rule.AsyncPredicate!(value, cancellationToken))
                {
                    continue;
                }

                (errors ??= new List<(ValidationError, IPropertyRule)>()).Add((this.CreateError(rule, value, options), rule));

                if (this.Behavior == ValidationBehavior.StopAtFirstError)
                {
                    break;
                }
            }

            return errors ?? SyncOutcome.NoErrors;
        }

        private ValidationError CreateError(PropertyRule<TProperty> rule, TProperty value, ValidationRunOptions options)
        {
            var arguments = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["PropertyName"] = this.DisplayNameProvider?.Invoke() ?? options.DisplayNameOf(this.PropertyName),
            };

            if (rule.Arguments != null)
            {
                foreach (var (key, argument) in rule.Arguments(value))
                {
                    arguments[key] = argument;
                }
            }

            if (rule.OtherPropertyName is { } otherPropertyName)
            {
                arguments["OtherPropertyName"] = options.DisplayNameOf(otherPropertyName);
            }

            var message = rule.MessageOverride != null
                ? rule.MessageOverride(value)
                : options.MessageProvider.GetMessage(rule.ErrorCode, arguments);

            return new ValidationError(this.PropertyName, rule.ErrorCode, message, arguments);
        }
    }
}
