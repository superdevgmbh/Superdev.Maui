namespace Superdev.Maui.Validation
{
    /// <summary>
    /// Builds the rule chain of one property. The rule methods — <c>NotEmpty()</c>, <c>MinimumLength(3)</c>,
    /// <c>Must(...)</c> — are extension methods in <see cref="PropertyRuleBuilderExtensions"/>; the members here
    /// shape the rule just written (<see cref="WithMessage(string)"/>, <see cref="WithErrorCode"/>, <see cref="InGroup"/>)
    /// or the property as a whole (<see cref="When"/>, <see cref="Unless"/>, <see cref="Alias(string[])"/>, <see cref="ValidateOn"/>).
    /// </summary>
    public sealed class PropertyRuleBuilder<TProperty>
    {
        private readonly PropertyRules<TProperty> rules;
        private readonly string? defaultGroup;
        private readonly Action rulesChanged;
        private readonly Action<string> groupNamed;

        internal PropertyRuleBuilder(
            PropertyRules<TProperty> rules,
            Func<TimeProvider> timeProvider,
            string? defaultGroup,
            Action rulesChanged,
            Action<string> groupNamed)
        {
            this.rules = rules;
            this.TimeProvider = timeProvider;
            this.defaultGroup = defaultGroup;
            this.rulesChanged = rulesChanged;
            this.groupNamed = groupNamed;
        }

        public string PropertyName => this.rules.PropertyName;

        /// <summary>
        /// The form's time provider, resolved when a rule runs, so that rules about "now" can be tested with a fixed clock.
        /// </summary>
        internal Func<TimeProvider> TimeProvider { get; }

        /// <summary>
        /// Appends a synchronous rule. The predicate returns <c>true</c> when the value is valid.
        /// </summary>
        public PropertyRuleBuilder<TProperty> Add(
            string errorCode,
            Func<TProperty, bool> predicate,
            Func<TProperty, IReadOnlyDictionary<string, object?>>? arguments = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
            ArgumentNullException.ThrowIfNull(predicate);

            return this.Append(new PropertyRule<TProperty>(errorCode) { Predicate = predicate, Arguments = arguments });
        }

        /// <summary>
        /// Appends an asynchronous rule. It runs once every synchronous rule of the property passed. A validation
        /// triggered by a change waits <paramref name="debounce"/> first and discards the run when the value changes
        /// again meanwhile; an explicit validation runs it at once.
        /// </summary>
        public PropertyRuleBuilder<TProperty> AddAsync(
            string errorCode,
            Func<TProperty, CancellationToken, ValueTask<bool>> predicate,
            TimeSpan? debounce = null,
            Func<TProperty, IReadOnlyDictionary<string, object?>>? arguments = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
            ArgumentNullException.ThrowIfNull(predicate);

            if (debounce is { } d && d < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(debounce), debounce, "The debounce must not be negative.");
            }

            return this.Append(new PropertyRule<TProperty>(errorCode)
            {
                AsyncPredicate = predicate,
                Debounce = debounce ?? TimeSpan.Zero,
                Arguments = arguments,
            });
        }

        /// <summary>Replaces the message of the rule just written.</summary>
        public PropertyRuleBuilder<TProperty> WithMessage(string message)
        {
            ArgumentNullException.ThrowIfNull(message);
            this.LastRuleOrThrow(nameof(this.WithMessage)).MessageOverride = _ => message;
            return this;
        }

        /// <summary>
        /// Replaces the message of the rule just written with one resolved while the rule runs — a localized
        /// resource depends on the culture of that moment, not of the moment the rule was declared.
        /// </summary>
        public PropertyRuleBuilder<TProperty> WithMessage(Func<string> message)
        {
            ArgumentNullException.ThrowIfNull(message);
            this.LastRuleOrThrow(nameof(this.WithMessage)).MessageOverride = _ => message();
            return this;
        }

        /// <summary>Replaces the message of the rule just written with one that can name the value which failed.</summary>
        public PropertyRuleBuilder<TProperty> WithMessage(Func<TProperty, string> message)
        {
            ArgumentNullException.ThrowIfNull(message);
            this.LastRuleOrThrow(nameof(this.WithMessage)).MessageOverride = message;
            return this;
        }

        /// <summary>Names the rule just written, so that a message provider or a test can tell which one failed.</summary>
        public PropertyRuleBuilder<TProperty> WithErrorCode(string errorCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
            this.LastRuleOrThrow(nameof(this.WithErrorCode)).ErrorCode = errorCode;
            return this;
        }

        /// <summary>Puts the rule just written into a group: it then runs only when the group is selected.</summary>
        public PropertyRuleBuilder<TProperty> InGroup(string group)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(group);
            this.LastRuleOrThrow(nameof(this.InGroup)).Groups.Add(group);
            this.groupNamed(group);
            this.rulesChanged();
            return this;
        }

        /// <summary>Puts the rule just written into several groups: it runs when any of them is selected.</summary>
        public PropertyRuleBuilder<TProperty> InGroups(params string[] groups)
        {
            ArgumentNullException.ThrowIfNull(groups);
            var rule = this.LastRuleOrThrow(nameof(this.InGroups));
            foreach (var group in groups)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(group, nameof(groups));
                rule.Groups.Add(group);
                this.groupNamed(group);
            }

            this.rulesChanged();
            return this;
        }

        /// <summary>
        /// Applies the property's rules only while <paramref name="condition"/> holds. When the condition turns off,
        /// the errors of these rules disappear; when it turns on again, the property is validated as the form's
        /// policy says.
        /// </summary>
        public PropertyRuleBuilder<TProperty> When(Func<bool> condition)
        {
            ArgumentNullException.ThrowIfNull(condition);
            this.AddCondition(condition);
            return this;
        }

        /// <summary>The inverse of <see cref="When"/>: applies the property's rules unless <paramref name="condition"/> holds.</summary>
        public PropertyRuleBuilder<TProperty> Unless(Func<bool> condition)
        {
            ArgumentNullException.ThrowIfNull(condition);
            this.AddCondition(() => !condition());
            return this;
        }

        private void AddCondition(Func<bool> condition)
        {
            this.rules.Conditions.Add(condition);

            // Known from the start, so that already the first flip counts as one.
            this.rules.LastApplies = this.rules.TryAppliesNow();
            this.rulesChanged();
        }

        /// <summary>
        /// Declares incoming error keys this property owns, so that errors set from outside — a server that names
        /// <c>DuplicateEmail</c>, an Identity error code — land on this field rather than on the form.
        /// </summary>
        public PropertyRuleBuilder<TProperty> Alias(params string[] keys)
        {
            ArgumentNullException.ThrowIfNull(keys);
            foreach (var key in keys)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(keys));
                this.rules.Aliases.Add(key);
            }

            return this;
        }

        /// <summary>
        /// Declares incoming error keys this property owns by predicate, e.g. every key starting with <c>Password</c>:
        /// <c>Alias(key => key.StartsWith("Password", StringComparison.OrdinalIgnoreCase))</c>.
        /// </summary>
        public PropertyRuleBuilder<TProperty> Alias(Func<string, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            this.rules.AliasPredicates.Add(predicate);
            return this;
        }

        /// <summary>
        /// The name the property has in messages. Without it, the message provider is asked for a translation of the
        /// property name, and the property name split into words is the fallback.
        /// </summary>
        public PropertyRuleBuilder<TProperty> WithDisplayName(string displayName)
        {
            ArgumentNullException.ThrowIfNull(displayName);
            this.rules.DisplayNameProvider = () => displayName;
            return this;
        }

        /// <summary>The name the property has in messages, resolved while a rule runs.</summary>
        public PropertyRuleBuilder<TProperty> WithDisplayName(Func<string> displayName)
        {
            ArgumentNullException.ThrowIfNull(displayName);
            this.rules.DisplayNameProvider = displayName;
            return this;
        }

        /// <summary>Overrides the form's <see cref="ViewModelValidation.Mode"/> for this property.</summary>
        public PropertyRuleBuilder<TProperty> ValidateOn(ValidationMode mode)
        {
            this.rules.ModeOverride = mode;
            this.rulesChanged();
            return this;
        }

        /// <summary>Whether the chain stops at its first failing rule (the default) or every failing rule reports.</summary>
        public PropertyRuleBuilder<TProperty> WithValidationBehavior(ValidationBehavior behavior)
        {
            this.rules.Behavior = behavior;
            this.rulesChanged();
            return this;
        }

        /// <summary>
        /// Re-validates this property when <paramref name="propertyName"/> changes — what a cross-field rule such as
        /// <c>EqualTo(() => this.Password)</c> registers for the other side.
        /// </summary>
        public PropertyRuleBuilder<TProperty> DependsOn(string propertyName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
            this.rules.Dependencies.Add(propertyName);
            return this;
        }

        internal PropertyRuleBuilder<TProperty> Append(PropertyRule<TProperty> rule)
        {
            if (this.defaultGroup != null)
            {
                rule.Groups.Add(this.defaultGroup);
                this.groupNamed(this.defaultGroup);
            }

            this.rules.Rules.Add(rule);
            this.rulesChanged();
            return this;
        }

        private PropertyRule<TProperty> LastRuleOrThrow(string operation)
        {
            return this.rules.LastRule
                ?? throw new InvalidOperationException($"{operation} applies to the rule just written, but no rule has been declared for '{this.PropertyName}' yet.");
        }
    }
}
