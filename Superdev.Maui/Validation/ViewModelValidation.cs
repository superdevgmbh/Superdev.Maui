using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Superdev.Maui.Internals;
using Superdev.Maui.Mvvm;

namespace Superdev.Maui.Validation
{
    /// <summary>
    /// The form state of a view model: its rules, the errors they produce, errors set from anywhere else, and per
    /// field whether the user has touched it, changed it or is waiting for an asynchronous rule. Rules are declared
    /// in <c>SetupValidation()</c> with <see cref="Property{TProperty}(Func{TProperty}, string?)"/>; the view model
    /// forwards its property changes, and <see cref="Mode"/> / <see cref="ReValidateMode"/> decide what runs when.
    /// </summary>
    /// <remarks>
    /// <para>
    /// State is owned by the context a validation was started on — the UI thread when a bound command or a setter
    /// starts it, the test's context in a test. Synchronous rules run inline there; an asynchronous rule marshals
    /// its result back before touching state. A lock makes reads from other threads safe; every collection handed
    /// out is a snapshot. Changes are expected from one context at a time: properties set from several threads at
    /// once can announce <see cref="IsValid"/>, <see cref="IsDirty"/>, <see cref="IsPending"/> or
    /// <see cref="HasErrors"/> out of order.
    /// </para>
    /// <para>
    /// A form included with <see cref="Include"/> inherits the parent's <see cref="Mode"/>, <see cref="ReValidateMode"/>,
    /// <see cref="TimeProvider"/> and <see cref="MessageProvider"/> unless it sets its own, runs the parent's active
    /// groups in addition to its own, and reports every change to the parent, whose <see cref="IsValid"/>,
    /// <see cref="HasErrors"/> and <see cref="ValidationErrors.All"/> include it.
    /// </para>
    /// </remarks>
    public class ViewModelValidation : BindableBase, INotifyDataErrorInfo, IErrorKeyRoutingSource
    {
        private static readonly IReadOnlyList<string> NoMessages = Array.Empty<string>();

        /// <summary>
        /// How often an explicit validation judges fields again which changed while it ran, before it gives up on
        /// values that keep changing — a rule which sets a property it depends on would otherwise never finish.
        /// </summary>
        private const int MaxRevalidationRounds = 3;

        private readonly Lock syncRoot = new Lock();
        private readonly Dictionary<string, FieldEntry> fields = new Dictionary<string, FieldEntry>(StringComparer.Ordinal);
        private readonly List<FieldEntry> fieldOrder = new List<FieldEntry>();
        private readonly Dictionary<string, FieldState> detachedFields = new Dictionary<string, FieldState>(StringComparer.Ordinal);
        private readonly List<ValidationError> formErrors = new List<ValidationError>();
        private readonly List<string> groupNames = new List<string>();
        private readonly List<ChildForm> children = new List<ChildForm>();
        private readonly List<RulePhase> rulePhases = new List<RulePhase>();
        private readonly HashSet<string> changedDuringAction = new HashSet<string>(StringComparer.Ordinal);

        private HashSet<string> activeGroups = new HashSet<string>(StringComparer.Ordinal);
        private WeakReference<ViewModelValidation>? parent;
        private IValidationMessageProvider? messageProvider;
        private TimeProvider? timeProvider;
        private ValidationMode? mode;
        private ReValidationMode? reValidateMode;
        private int validating;
        private int actionPhase;
        private bool hasValidated;
        private bool? isValid;
        private bool hasErrors;
        private bool isDirty;
        private bool isPending;

        public ViewModelValidation()
        {
            this.Errors = new ValidationErrors(this.MessagesOf, this.FormMessages, this.AllByProperty, this.AllMessages);
            this.Fields = new FieldStates(this.FieldOf, this.AllFields);
        }

        /// <summary>
        /// The message provider a form uses unless it or a parent sets <see cref="MessageProvider"/>. <c>UseSuperdevMaui</c>
        /// installs one backed by the app's translation provider.
        /// </summary>
        public static IValidationMessageProvider DefaultMessageProvider { get; set; } = new DefaultValidationMessageProvider();

        /// <summary>The message provider of this form: its own, a parent's, or <see cref="DefaultMessageProvider"/>. Set <c>null</c> to inherit again.</summary>
        [AllowNull]
        public IValidationMessageProvider MessageProvider
        {
            get => this.messageProvider ?? this.Parent?.MessageProvider ?? DefaultMessageProvider;
            set => this.messageProvider = value;
        }

        /// <summary>The clock rules about "now" and debounces run on: this form's, a parent's, or the system clock. Tests install a fake.</summary>
        [AllowNull]
        public TimeProvider TimeProvider
        {
            get => this.timeProvider ?? this.Parent?.TimeProvider ?? TimeProvider.System;
            set => this.timeProvider = value;
        }

        /// <summary>
        /// When a field is validated for the first time. <see cref="ValidationMode.Manual"/> by default; an included
        /// form follows its parent unless it sets its own.
        /// </summary>
        public ValidationMode Mode
        {
            get => this.mode ?? this.Parent?.Mode ?? ValidationMode.Manual;
            set => this.mode = value;
        }

        /// <summary>
        /// When a field with an error, or any field once the form was validated, is validated again.
        /// <see cref="ReValidationMode.OnChange"/> by default; an included form follows its parent unless it sets its own.
        /// </summary>
        public ReValidationMode ReValidateMode
        {
            get => this.reValidateMode ?? this.Parent?.ReValidateMode ?? ReValidationMode.OnChange;
            set => this.reValidateMode = value;
        }

        /// <summary>The messages, shaped for XAML: <c>Errors[Name]</c>, <c>Errors.Form</c>, <c>Errors.All</c>.</summary>
        public ValidationErrors Errors { get; }

        /// <summary>The state of each field: <c>Fields[Name].IsTouched</c>, <c>.IsDirty</c>, <c>.IsPending</c>, <c>.Errors</c>.</summary>
        public FieldStates Fields { get; }

        /// <summary>The groups any rule belongs to, in the order they were first named.</summary>
        public IReadOnlyList<ValidationGroup> Groups
        {
            get
            {
                lock (this.syncRoot)
                {
                    var groups = this.groupNames.Select(name => new ValidationGroup(name)).ToList();
                    var byName = groups.ToDictionary(group => group.Name, StringComparer.Ordinal);

                    foreach (var entry in this.fieldOrder)
                    {
                        foreach (var chain in entry.Chains)
                        {
                            foreach (var ruleGroups in chain.RuleGroups())
                            {
                                foreach (var name in ruleGroups)
                                {
                                    if (byName.TryGetValue(name, out var group))
                                    {
                                        group.Register(entry.State.PropertyName);
                                    }
                                }
                            }
                        }
                    }

                    return groups;
                }
            }
        }

        /// <summary>
        /// The groups whose rules run on a property change and count for <see cref="IsValid"/>: this form's own and,
        /// for an included form, its parent's. Setting it replaces this form's own groups. Selecting groups for a
        /// <see cref="ValidateAsync(IEnumerable{string}?, CancellationToken)"/> also makes them active.
        /// </summary>
        public IReadOnlySet<string> ActiveGroups
        {
            get => this.EffectiveActiveGroups();
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                this.SetActiveGroupsCore(value);
            }
        }

        /// <summary>Any error, from a rule or set from outside, on this form or a child.</summary>
        public bool HasErrors
        {
            get
            {
                lock (this.syncRoot)
                {
                    if (this.formErrors.Count > 0 || this.fieldOrder.Any(e => e.RuleErrors.Count > 0 || e.SetErrors.Count > 0))
                    {
                        return true;
                    }
                }

                return this.LiveChildren().Any(c => c.Form.HasErrors);
            }
        }

        /// <summary>
        /// Whether a full validation would pass right now, computed silently from the synchronous rules — what a
        /// button binds to when it should be disabled until the form is complete. Nothing is shown by reading it. The
        /// last verdict of an asynchronous rule counts as long as the value it judged is unchanged, and a field waiting
        /// for one is not valid yet. Errors set from
        /// outside do not count: the next <see cref="ValidateAsync(Func{CancellationToken, Task{ValidationResult}}, IEnumerable{string}?, CancellationToken)"/> replaces them.
        /// </summary>
        public bool IsValid
        {
            get
            {
                var computed = this.ComputeIsValid();
                this.isValid ??= computed;
                return computed;
            }
        }

        /// <summary>
        /// Any field, here or in a child, differs from its value at setup or at the last <see cref="Reset"/>. Call
        /// <see cref="Reset"/> once the view model has loaded its data, so that the loaded values are the baseline.
        /// </summary>
        public bool IsDirty => this.ComputeIsDirty();

        /// <summary>An asynchronous rule is in flight on this form or a child.</summary>
        public bool IsPending => this.ComputeIsPending();

        /// <summary>An explicit validation of this form — rules and action — is in progress.</summary>
        public bool IsValidating => Volatile.Read(ref this.validating) != 0;

        /// <summary><see cref="ValidateAsync(IEnumerable{string}?, CancellationToken)"/> has run at least once since setup or the last <see cref="Reset"/>.</summary>
        public bool HasValidated => this.hasValidated;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <summary>Raised for the first invalid field after an explicit validation, so that the control wired to it takes focus.</summary>
        public event EventHandler<FocusRequestedEventArgs>? FocusRequested;

        /// <summary>
        /// Raised when a rule threw while a property was validated after a change — the path no caller awaits. A view
        /// model can turn it into its <c>ViewModelError</c>.
        /// </summary>
        public event EventHandler<ValidationFailedEventArgs>? ValidationFailed;

        /// <summary>Anything a parent aggregates changed: errors, validity, dirty or pending state.</summary>
        internal event EventHandler<EventArgs>? StateChanged;

        /// <summary>The form which included this one, held weakly: a long-lived child never keeps a page's form alive.</summary>
        private ViewModelValidation? Parent => Volatile.Read(ref this.parent) is { } link && link.TryGetTarget(out var form) ? form : null;

        private bool IsInRulePhase => this.ActiveRulePhases() != null;

        private bool IsInActionPhase => Volatile.Read(ref this.actionPhase) > 0 || (this.Parent?.IsInActionPhase ?? false);

        #region Declaring rules

        /// <summary>
        /// Starts the rule chain of a property. The name is taken from the accessor's text, so
        /// <c>Property(() => this.UserName)</c> names <c>UserName</c>; anything but a plain property access is rejected.
        /// A property declared without rules still receives the errors set for it from outside.
        /// </summary>
        public PropertyRuleBuilder<TProperty> Property<TProperty>(
            Func<TProperty> accessor,
            [CallerArgumentExpression(nameof(accessor))] string? accessorExpression = null)
        {
            return this.PropertyCore(PropertyNameParser.Parse(accessorExpression), accessor, null);
        }

        /// <summary>Starts the rule chain of a property, naming it explicitly.</summary>
        public PropertyRuleBuilder<TProperty> Property<TProperty>(string propertyName, Func<TProperty> accessor)
        {
            return this.PropertyCore(propertyName, accessor, null);
        }

        /// <summary>Declares rules which run only when <paramref name="name"/> is among the selected groups.</summary>
        public void Group(string name, Action<IValidationGroupBuilder> configure)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(configure);

            this.OnGroupNamed(name);
            configure(new ValidationGroupBuilder(this, name));
        }

        /// <summary>
        /// Includes the validation of a child view model, resolved when needed, so that a child created later is fine.
        /// The child validates with the parent, inherits its policy and active groups, its errors and state count for
        /// the parent, and an incoming key such as <c>Address.Street</c> — or a key only the child owns — finds it.
        /// </summary>
        public void Include(string name, Func<ViewModelValidation?> child)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(child);

            lock (this.syncRoot)
            {
                if (this.children.Any(c => string.Equals(c.Name, name, StringComparison.Ordinal)))
                {
                    throw new InvalidOperationException($"A child named '{name}' is already included.");
                }

                this.children.Add(new ChildForm(name, child));
            }

            this.RefreshDerivedState();
        }

        internal PropertyRuleBuilder<TProperty> PropertyCore<TProperty>(string propertyName, Func<TProperty> accessor, string? group)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
            ArgumentNullException.ThrowIfNull(accessor);

            var chain = new PropertyRules<TProperty>(propertyName, accessor);
            var initialValue = TryGetValue(chain);
            var baseline = Baseline(initialValue);
            FieldEntry entry;
            bool added;

            lock (this.syncRoot)
            {
                added = !this.fields.ContainsKey(propertyName);
                entry = this.GetOrAddEntry(propertyName);
                entry.Chains.Add(chain);

                if (!entry.HasInitialValue)
                {
                    entry.InitialValue = baseline;
                    entry.HasInitialValue = true;
                }
            }

            if (added)
            {
                this.WatchCollection(entry, initialValue);
                this.Fields.NotifyFieldAdded();
            }

            var builder = new PropertyRuleBuilder<TProperty>(chain, () => this.TimeProvider, group, this.RefreshDerivedState, this.OnGroupNamed);
            this.RefreshDerivedState();
            return builder;
        }

        private void OnGroupNamed(string group)
        {
            lock (this.syncRoot)
            {
                if (!this.groupNames.Contains(group, StringComparer.Ordinal))
                {
                    this.groupNames.Add(group);
                }
            }
        }

        #endregion

        #region Validating

        /// <summary>
        /// Validates every field, own and included, marks every field touched, shows the errors and focuses the first
        /// invalid field. Returns <c>true</c> when everything passed and no error set from outside stands. A second call
        /// while one is in progress returns <c>false</c> without running.
        /// </summary>
        /// <param name="groups">
        /// The groups whose rules run in addition to the ungrouped ones; they become the active groups. <c>null</c>
        /// keeps the groups that are active now — pass an empty list to run the ungrouped rules only.
        /// </param>
        /// <param name="cancellationToken">Cancels the validation; a cancelled validation throws <see cref="OperationCanceledException"/>.</param>
        public Task<bool> ValidateAsync(IEnumerable<string>? groups = null, CancellationToken cancellationToken = default)
        {
            return this.ValidateCoreAsync(null, groups, cancellationToken);
        }

        /// <summary>
        /// Validates as <see cref="ValidateAsync(IEnumerable{string}?, CancellationToken)"/> does and, when the rules
        /// passed, runs <paramref name="action"/> — the save, the API call — whatever errors were set from outside
        /// before, since the action's result replaces them. A result which did not succeed is shown through
        /// <see cref="SetErrors(ValidationResult)"/>; a successful one clears every error set from outside, on this form
        /// and its children. An action which throws propagates to the caller.
        /// </summary>
        public Task<bool> ValidateAsync(
            Func<CancellationToken, Task<ValidationResult>> action,
            IEnumerable<string>? groups = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(action);
            return this.ValidateCoreAsync(action, groups, cancellationToken);
        }

        /// <summary>Validates one field now, asynchronous rules included, and shows its errors.</summary>
        public async Task<bool> ValidatePropertyAsync(string propertyName, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

            var entry = this.TryGetEntry(propertyName);
            if (entry == null)
            {
                return true;
            }

            var isFieldValid = await this.ValidateFieldAsync(entry, immediate: true, cancellationToken) == FieldVerdict.Passed;
            cancellationToken.ThrowIfCancellationRequested();
            this.RefreshDerivedState();
            return isFieldValid;
        }

        private async Task<bool> ValidateCoreAsync(
            Func<CancellationToken, Task<ValidationResult>>? action,
            IEnumerable<string>? groups,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (Interlocked.CompareExchange(ref this.validating, 1, 0) != 0)
            {
                return false;
            }

            try
            {
                this.RaisePropertyChanged(nameof(this.IsValidating));

                bool rulesPassed;
                var phase = new RulePhase();
                lock (this.syncRoot)
                {
                    this.rulePhases.Add(phase);
                }

                try
                {
                    rulesPassed = await this.RunRulesAsync(groups, phase, cancellationToken);
                    rulesPassed = await this.JudgeChangesDuringRulesAsync(phase, rulesPassed, cancellationToken);
                }
                finally
                {
                    lock (this.syncRoot)
                    {
                        this.rulePhases.Remove(phase);
                    }

                    // Whatever is still unjudged — after a cancellation, a failure, or values that kept changing — goes
                    // back to the policy.
                    HandOverToThePolicy(phase);
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (!rulesPassed)
                {
                    this.RequestFocusOnFirstError(ruleErrorsOnly: true);
                    return false;
                }

                if (action == null)
                {
                    // Without an action there is no fresh verdict: an error set from outside still stands.
                    if (this.HasErrors)
                    {
                        this.RequestFocusOnFirstError(ruleErrorsOnly: false);
                        return false;
                    }

                    return true;
                }

                ValidationResult result;
                Interlocked.Increment(ref this.actionPhase);
                try
                {
                    result = await action(cancellationToken)
                        ?? throw new InvalidOperationException("The action must return a ValidationResult; use ValidationResult.Success for a successful action.");
                }
                finally
                {
                    Interlocked.Decrement(ref this.actionPhase);
                }

                if (result.Succeeded)
                {
                    // The action's verdict is the fresh one: whatever was set before it is stale, here and in children.
                    this.ClearSetErrorsInTree();
                    return true;
                }

                this.SetErrorsCore(result.Errors);

                // A field the user changed while the action ran was judged by its old value.
                this.DropSetErrorsChangedDuringAction();
                this.RequestFocusOnFirstError(ruleErrorsOnly: false);
                return false;
            }
            finally
            {
                Interlocked.Exchange(ref this.validating, 0);
                this.ForgetChangesDuringAction();
                this.RaisePropertyChanged(nameof(this.IsValidating));
            }
        }

        /// <summary>
        /// Runs the rules of this form and its children: marks the fields touched and shows the errors. Errors set from
        /// outside take no part — deciding about them is up to the call that started the validation.
        /// </summary>
        private async Task<bool> RunRulesAsync(IEnumerable<string>? groups, RulePhase phase, CancellationToken cancellationToken)
        {
            if (groups != null)
            {
                this.SetActiveGroupsCore(groups);
            }

            foreach (var entry in this.AllEntries())
            {
                entry.State.SetTouched(true);
            }

            if (!this.hasValidated)
            {
                this.hasValidated = true;
                this.RaisePropertyChanged(nameof(this.HasValidated));
            }

            var entries = this.EntriesWithRules();
            var fieldRuns = new Task<FieldVerdict>[entries.Length];
            for (var i = 0; i < entries.Length; i++)
            {
                fieldRuns[i] = this.ValidateFieldAsync(entries[i], immediate: true, cancellationToken);
            }

            var childRuns = new List<Task<bool>>();
            foreach (var (_, child) in this.LiveChildren())
            {
                childRuns.Add(child.RunRulesAsync(null, phase, cancellationToken));
            }

            await Task.WhenAll(fieldRuns.Concat<Task>(childRuns));
            var verdicts = await Task.WhenAll(fieldRuns);
            var childrenPassed = await Task.WhenAll(childRuns);

            if (!cancellationToken.IsCancellationRequested)
            {
                // A run which another one replaced judged nothing: the validation judges the field again before its verdict.
                for (var i = 0; i < entries.Length; i++)
                {
                    if (verdicts[i] == FieldVerdict.Abandoned)
                    {
                        phase.FieldChanged(this, entries[i].State.PropertyName);
                    }
                }
            }

            this.NotifyErrorsChanged();
            this.RefreshDerivedState();
            return verdicts.All(verdict => verdict == FieldVerdict.Passed) && childrenPassed.All(passed => passed);
        }

        /// <summary>
        /// Runs the rules of one field: the synchronous ones first, shown at once, then — if they all passed — the
        /// asynchronous ones, while the field is pending. A run triggered by a change honours debounces and is discarded
        /// when the value changes again meanwhile; an explicit run validates at once. An explicit run joins one already
        /// judging the same value, so that two validations of overlapping forms — a child's own and its parent's —
        /// share it instead of replacing each other's.
        /// </summary>
        private Task<FieldVerdict> ValidateFieldAsync(FieldEntry entry, bool immediate, CancellationToken cancellationToken)
        {
            PropertyRules[] chains;
            ExplicitRun? running;
            RunSequencer sequencer;

            lock (this.syncRoot)
            {
                chains = entry.Chains.ToArray();
                running = entry.ExplicitRun;
                sequencer = entry.Sequencer ??= new RunSequencer();
            }

            var value = chains.Length > 0 ? TryGetValue(chains[0]) : null;
            var groups = this.EffectiveActiveGroups();

            // Joined only while it can still judge, and only when it judges what this run would: the same value under
            // the same groups.
            if (immediate
                && running is { Task.IsCompleted: false }
                && !running.RunToken.IsCancellationRequested
                && running.Groups.SetEquals(groups)
                && AreSameValue(value, running.Value))
            {
                return this.JoinAsync(running.Task, entry, cancellationToken);
            }

            var runToken = sequencer.Begin(cancellationToken);
            var run = this.RunFieldAsync(entry, chains, value, groups, immediate, sequencer, runToken, cancellationToken);
            if (immediate && !run.IsCompleted)
            {
                // Still running, so asynchronous: another explicit validation may join it.
                lock (this.syncRoot)
                {
                    entry.ExplicitRun = new ExplicitRun(run, Baseline(value), groups, runToken);
                }
            }

            return run;
        }

        /// <summary>
        /// Waits for the run another explicit validation started. Should that run be abandoned — its owner cancelled
        /// it, say — while this one still wants a verdict, the field is judged anew.
        /// </summary>
        private async Task<FieldVerdict> JoinAsync(Task<FieldVerdict> run, FieldEntry entry, CancellationToken cancellationToken)
        {
            var verdict = await run.WaitAsync(cancellationToken);
            return verdict == FieldVerdict.Abandoned && !cancellationToken.IsCancellationRequested
                ? await this.ValidateFieldAsync(entry, immediate: true, cancellationToken)
                : verdict;
        }

        private async Task<FieldVerdict> RunFieldAsync(
            FieldEntry entry,
            PropertyRules[] chains,
            object? value,
            IReadOnlySet<string> groups,
            bool immediate,
            RunSequencer sequencer,
            CancellationToken runToken,
            CancellationToken cancellationToken)
        {
            var options = new ValidationRunOptions
            {
                ActiveGroups = groups,
                MessageProvider = this.MessageProvider,
                TimeProvider = this.TimeProvider,
                DisplayNameOf = this.DisplayNameOf,
                Immediate = immediate,
            };

            var syncErrors = new List<(ValidationError Error, IPropertyRule Rule)>();
            var asyncChains = new List<PropertyRules>();

            foreach (var chain in chains)
            {
                var outcome = chain.ValidateSync(options);
                syncErrors.AddRange(outcome.Errors);

                if (outcome.AsyncRulesPending)
                {
                    asyncChains.Add(chain);
                }

                if (syncErrors.Count > 0 && chain.Behavior == ValidationBehavior.StopAtFirstError)
                {
                    break;
                }
            }

            if (!sequencer.IsCurrent(runToken))
            {
                return FieldVerdict.Abandoned;
            }

            if (syncErrors.Count > 0 || asyncChains.Count == 0)
            {
                this.ReplaceRuleErrors(entry, syncErrors);
                return syncErrors.Count == 0 ? FieldVerdict.Passed : FieldVerdict.Failed;
            }

            // An asynchronous verdict holds for the value it judged, and for that value only: a collection's items as
            // they are now, before the rules await anything.
            var judgedValue = Baseline(value);

            // The synchronous rules passed: the field waits for the asynchronous ones, and shows no error meanwhile.
            // Pending comes first, so that IsValid does not flip to true in between.
            var isFirstPendingRun = Interlocked.Increment(ref entry.PendingRuns) == 1;

            try
            {
                if (isFirstPendingRun)
                {
                    entry.State.SetPending(true);
                }

                this.ReplaceRuleErrors(entry, SyncOutcome.NoErrors);
                this.RefreshDerivedState();

                var asyncErrors = new List<(ValidationError Error, IPropertyRule Rule)>();
                foreach (var chain in asyncChains)
                {
                    asyncErrors.AddRange(await chain.ValidateAsyncRulesAsync(options, runToken));

                    if (asyncErrors.Count > 0 && chain.Behavior == ValidationBehavior.StopAtFirstError)
                    {
                        break;
                    }
                }

                if (!sequencer.IsCurrent(runToken))
                {
                    return FieldVerdict.Abandoned;
                }

                this.DropVerdictsWhichNoLongerCount(asyncErrors, asyncChains);

                lock (this.syncRoot)
                {
                    entry.AsyncVerdictValue = judgedValue;
                }

                this.ReplaceRuleErrors(entry, asyncErrors);
                return asyncErrors.Count == 0 ? FieldVerdict.Passed : FieldVerdict.Failed;
            }
            catch (OperationCanceledException) when (!sequencer.IsCurrent(runToken) || cancellationToken.IsCancellationRequested)
            {
                // Superseded by a later run, or cancelled by the caller: the errors shown stay as they are.
                return FieldVerdict.Abandoned;
            }
            finally
            {
                if (Interlocked.Decrement(ref entry.PendingRuns) == 0)
                {
                    entry.State.SetPending(false);
                    this.RefreshDerivedState();
                }
            }
        }

        /// <summary>
        /// A condition which turned off, or a group which was deselected, while an asynchronous rule ran takes the
        /// rule's verdict with it. A condition which throws keeps it.
        /// </summary>
        private void DropVerdictsWhichNoLongerCount(List<(ValidationError Error, IPropertyRule Rule)> errors, IReadOnlyList<PropertyRules> chains)
        {
            if (errors.Count == 0)
            {
                return;
            }

            var active = this.EffectiveActiveGroups();
            errors.RemoveAll(pair => !pair.Rule.IsActive(active));

            foreach (var chain in chains)
            {
                if (chain.TryAppliesNow() == false)
                {
                    errors.RemoveAll(pair => chain.Contains(pair.Rule));
                }
            }
        }

        private void ReplaceRuleErrors(FieldEntry entry, IReadOnlyList<(ValidationError Error, IPropertyRule Rule)> errors)
        {
            lock (this.syncRoot)
            {
                entry.RuleErrors.Clear();
                entry.RuleErrors.AddRange(errors);
            }

            this.PublishField(entry);
        }

        /// <summary>
        /// Keeps the form in step with a property change of the view model: drops the errors set from outside for the
        /// field — the verdict on the old value is stale — updates its dirty state, notices conditions that turned on
        /// or off, and validates the field, the fields depending on it and those whose condition flipped according to
        /// <see cref="Mode"/> and <see cref="ReValidateMode"/>. While an explicit validation runs its rules, they are
        /// left to it instead. The returned task does not fault: this runs on every property change of a view model,
        /// so a rule which throws is reported through <see cref="OnValidationFailed"/>.
        /// </summary>
        internal async Task HandlePropertyChangeAsync(string? propertyName)
        {
            try
            {
                var affected = new List<FieldEntry>();
                var phases = this.ActiveRulePhases();

                if (string.IsNullOrEmpty(propertyName))
                {
                    // Everything changed: bring the dirty state and the watched collections up to date, validate nothing new.
                    foreach (var entry in this.AllEntries())
                    {
                        var value = this.CurrentValue(entry);
                        this.WatchCollection(entry, value);
                        this.UpdateDirty(entry, value);
                    }
                }
                else
                {
                    var entry = this.TryGetEntry(propertyName);
                    if (entry != null)
                    {
                        var value = this.CurrentValue(entry);
                        this.DropSetErrors(entry);
                        this.WatchCollection(entry, value);
                        this.UpdateDirty(entry, value);

                        if (this.IsInActionPhase)
                        {
                            lock (this.syncRoot)
                            {
                                this.changedDuringAction.Add(propertyName);
                            }
                        }

                        affected.Add(entry);
                    }

                    foreach (var dependent in this.DependentsOf(propertyName))
                    {
                        if (!affected.Contains(dependent))
                        {
                            affected.Add(dependent);
                        }
                    }

                    if (this.IsChildName(propertyName))
                    {
                        // An included view model was replaced: the new one is linked now, not on the next query that
                        // happens to reach it, and its errors join or leave this form's. A running explicit validation
                        // never saw it, so it validates the new one before its verdict.
                        _ = this.LiveChildren();
                        foreach (var phase in phases ?? Enumerable.Empty<RulePhase>())
                        {
                            phase.ChildReplaced(this, propertyName);
                        }

                        this.NotifyErrorsChanged();
                    }
                }

                this.ApplyConditionChanges(affected);

                if (phases != null)
                {
                    // Every explicit validation running over this form judged these fields by their old values: each
                    // judges them again before its verdict, whatever the policy says.
                    foreach (var phase in phases)
                    {
                        foreach (var field in affected)
                        {
                            phase.FieldChanged(this, field.State.PropertyName);
                        }
                    }
                }
                else
                {
                    var targets = affected.FindAll(this.ShouldValidateOnChange);
                    if (targets.Count > 0)
                    {
                        // Started one after the other, so that every field's synchronous verdict shows at once; awaited
                        // together, so that no field waits for another one's asynchronous rule.
                        var runs = new Task<FieldVerdict>[targets.Count];
                        for (var i = 0; i < targets.Count; i++)
                        {
                            runs[i] = this.ValidateFieldAsync(targets[i], immediate: false, CancellationToken.None);
                        }

                        await Task.WhenAll(runs);
                    }
                }

                this.RefreshDerivedState();
            }
            catch (Exception ex)
            {
                this.OnValidationFailed(propertyName, ex);
            }
        }

        /// <summary>
        /// Called when a rule threw while <paramref name="propertyName"/> was validated after a change. The base
        /// implementation traces the exception and raises <see cref="ValidationFailed"/>.
        /// </summary>
        protected virtual void OnValidationFailed(string? propertyName, Exception exception)
        {
            Trace.WriteLine($"Validation of property \"{propertyName}\" failed with exception: {exception}");
            this.ValidationFailed?.Invoke(this, new ValidationFailedEventArgs(propertyName, exception));
        }

        /// <summary>
        /// Judges again, at once and whatever the policy, what changed under the validation while its rules ran — fields
        /// edited here or in a child, child view models replaced, runs another validation replaced — since its verdict
        /// was on values which are gone. Repeats while changes keep coming in, for <see cref="MaxRevalidationRounds"/>
        /// rounds at most. Returns whether the rules pass for the values as they are now.
        /// </summary>
        private async Task<bool> JudgeChangesDuringRulesAsync(RulePhase phase, bool passed, CancellationToken cancellationToken)
        {
            // Once cancelled, the changes are left to the policy, which takes them over when the rule phase ends.
            for (var round = 0; !cancellationToken.IsCancellationRequested && phase.HasChanges; round++)
            {
                if (round == MaxRevalidationRounds)
                {
                    // The values keep changing under the validation: it cannot vouch for them.
                    return false;
                }

                var (fields, children) = phase.Take();
                var runs = new List<Task<bool>>();
                foreach (var (form, name) in fields)
                {
                    if (form.TryGetEntry(name) is { } entry)
                    {
                        runs.Add(JudgeAsync(form, entry, phase, cancellationToken));
                    }
                }

                // A child view model which replaced another one was never seen by the rules: it is validated as a whole.
                foreach (var (form, name) in children)
                {
                    foreach (var (childName, child) in form.LiveChildren())
                    {
                        if (string.Equals(childName, name, StringComparison.Ordinal))
                        {
                            runs.Add(child.RunRulesAsync(null, phase, cancellationToken));
                        }
                    }
                }

                await Task.WhenAll(runs);

                // A field still waiting for another validation's asynchronous rule is judged in the next round, which
                // joins that run or replaces it; it does not fail this one.
                this.RecordPendingFields(phase);

                // A field which failed may pass now, and the other way round: what the fields show now is the verdict.
                passed = this.RulesPassInTree();
            }

            return passed;

            static async Task<bool> JudgeAsync(ViewModelValidation form, FieldEntry entry, RulePhase phase, CancellationToken cancellationToken)
            {
                var verdict = await form.ValidateFieldAsync(entry, immediate: true, cancellationToken);
                if (verdict == FieldVerdict.Abandoned)
                {
                    // Cancelled or replaced before the field was judged: the next round judges it, or, once the
                    // validation is cancelled, the policy.
                    phase.FieldChanged(form, entry.State.PropertyName);
                }

                form.RefreshDerivedState();
                return verdict == FieldVerdict.Passed;
            }
        }

        private void RecordPendingFields(RulePhase phase)
        {
            foreach (var entry in this.AllEntries())
            {
                if (Volatile.Read(ref entry.PendingRuns) > 0)
                {
                    phase.FieldChanged(this, entry.State.PropertyName);
                }
            }

            foreach (var (_, child) in this.LiveChildren())
            {
                child.RecordPendingFields(phase);
            }
        }

        /// <summary>No rule error, and no field waiting for an asynchronous rule, here or in a child.</summary>
        private bool RulesPassInTree()
        {
            lock (this.syncRoot)
            {
                if (this.fieldOrder.Any(e => e.RuleErrors.Count > 0 || Volatile.Read(ref e.PendingRuns) > 0))
                {
                    return false;
                }
            }

            return this.LiveChildren().All(c => c.Form.RulesPassInTree());
        }

        /// <summary>
        /// Validates, according to the policy, the fields which changed while an explicit validation ran its rules and
        /// which it did not judge again — after a cancellation, a failure, or values that kept changing. A field another
        /// explicit validation is still running its rules over is left to that one. A child view model which arrived
        /// meanwhile is new: like any new view model, it waits for its policy.
        /// </summary>
        private static void HandOverToThePolicy(RulePhase phase)
        {
            var (fields, _) = phase.Take();
            foreach (var (form, name) in fields)
            {
                if (form.ActiveRulePhases() is { } others)
                {
                    foreach (var other in others)
                    {
                        other.FieldChanged(form, name);
                    }

                    continue;
                }

                if (form.TryGetEntry(name) is { } entry && form.ShouldValidateOnChange(entry))
                {
                    _ = form.ValidateInBackgroundAsync(entry, name);
                }
            }
        }

        /// <summary>The explicit validations running their rules over this form: its own and its ancestors'.</summary>
        private List<RulePhase>? ActiveRulePhases()
        {
            List<RulePhase>? phases = null;
            for (var form = this; form != null; form = form.Parent)
            {
                lock (form.syncRoot)
                {
                    if (form.rulePhases.Count > 0)
                    {
                        (phases ??= new List<RulePhase>()).AddRange(form.rulePhases);
                    }
                }
            }

            return phases;
        }

        private async Task ValidateInBackgroundAsync(FieldEntry entry, string propertyName)
        {
            try
            {
                await this.ValidateFieldAsync(entry, immediate: false, CancellationToken.None);
                this.RefreshDerivedState();
            }
            catch (Exception ex)
            {
                this.OnValidationFailed(propertyName, ex);
            }
        }

        #endregion

        #region Touched, dirty, reset

        /// <summary>
        /// Marks a field as touched — the user left it — and validates it when <see cref="Mode"/> or
        /// <see cref="ReValidateMode"/> say so. The control wired to the field calls this when it loses focus.
        /// </summary>
        public void MarkTouched(string propertyName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

            var entry = this.TryGetEntry(propertyName);
            if (entry == null)
            {
                return;
            }

            entry.State.SetTouched(true);

            if (this.IsInRulePhase || !this.ShouldValidateOnBlur(entry))
            {
                return;
            }

            _ = this.ValidateOnBlurAsync(entry, propertyName);
        }

        public void MarkAllTouched()
        {
            foreach (var entry in this.AllEntries())
            {
                entry.State.SetTouched(true);
            }

            foreach (var (_, child) in this.LiveChildren())
            {
                child.MarkAllTouched();
            }
        }

        public void ResetTouched()
        {
            foreach (var entry in this.AllEntries())
            {
                entry.State.SetTouched(false);
            }

            foreach (var (_, child) in this.LiveChildren())
            {
                child.ResetTouched();
            }
        }

        /// <summary>Removes every shown error, rule and set alike, on this form and its children. Touched state is untouched.</summary>
        public void ClearErrors()
        {
            foreach (var entry in this.AllEntries())
            {
                lock (this.syncRoot)
                {
                    entry.RuleErrors.Clear();
                    entry.SetErrors.Clear();
                }

                entry.Sequencer?.Cancel();
                entry.State.SetErrors(Array.Empty<ValidationError>());
            }

            lock (this.syncRoot)
            {
                this.formErrors.Clear();
            }

            foreach (var (_, child) in this.LiveChildren())
            {
                child.ClearErrors();
            }

            this.NotifyErrorsChanged();
            this.RefreshDerivedState();
        }

        /// <summary>
        /// Back to pristine: no errors, nothing touched, the current values become the ones <see cref="IsDirty"/>
        /// compares against, and <see cref="HasValidated"/> is <c>false</c> again. Cascades to the children. Call it
        /// once the view model has loaded its data.
        /// </summary>
        public void Reset()
        {
            foreach (var entry in this.AllEntries())
            {
                PropertyRules[] chains;
                lock (this.syncRoot)
                {
                    entry.RuleErrors.Clear();
                    entry.SetErrors.Clear();
                    chains = entry.Chains.ToArray();
                }

                // The values as they are now become the baseline, and so does whether each condition holds.
                var initialValue = chains.Length > 0 ? TryGetValue(chains[0]) : null;
                foreach (var chain in chains)
                {
                    chain.LastApplies = chain.TryAppliesNow();
                }

                var baseline = Baseline(initialValue);
                lock (this.syncRoot)
                {
                    entry.InitialValue = baseline;
                    entry.HasInitialValue = chains.Length > 0;
                }

                this.WatchCollection(entry, initialValue);
                entry.Sequencer?.Cancel();
                entry.State.SetErrors(Array.Empty<ValidationError>());
                entry.State.SetTouched(false);
                entry.State.SetDirty(false);
            }

            lock (this.syncRoot)
            {
                this.formErrors.Clear();
            }

            if (this.hasValidated)
            {
                this.hasValidated = false;
                this.RaisePropertyChanged(nameof(this.HasValidated));
            }

            foreach (var (_, child) in this.LiveChildren())
            {
                child.Reset();
            }

            this.NotifyErrorsChanged();
            this.RefreshDerivedState();
        }

        #endregion

        #region Errors set from outside

        /// <summary>
        /// Replaces the errors set from outside — a server's answer, a pre-flight check, a failed operation, any
        /// property → messages map — with <paramref name="errors"/>. A key lands on the field that owns it (by name,
        /// or by <c>Alias</c>), on a child (<c>Address.Street</c>, or a field only that child has), or else on
        /// <see cref="ValidationErrors.Form"/>; nothing is dropped. A message left empty becomes the default message of
        /// the error code ("Email is not valid."); only a form-level one without a key, which has nothing to name, is
        /// dropped. An error stays until its field changes or the next call replaces it.
        /// </summary>
        public void SetErrors<TMessages>(IEnumerable<KeyValuePair<string, TMessages>> errors)
            where TMessages : IEnumerable<string>
        {
            ArgumentNullException.ThrowIfNull(errors);

            var list = new List<ValidationError>();
            foreach (var (key, messages) in errors)
            {
                if (messages == null)
                {
                    continue;
                }

                foreach (var message in messages)
                {
                    list.Add(new ValidationError(key ?? string.Empty, ValidationErrorCodes.Custom, message ?? string.Empty));
                }
            }

            this.SetErrorsCore(list);
        }

        /// <summary>
        /// Replaces the errors set from outside with those of a result, routed as <see cref="SetErrors{TMessages}"/>
        /// routes them. Each error keeps its error code and arguments.
        /// </summary>
        public void SetErrors(ValidationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            this.SetErrorsCore(result.Errors);
        }

        /// <summary>Sets the error of one field, leaving what was set for other fields as it is.</summary>
        public void SetError(string propertyName, string message)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);

            var route = ErrorKeyRouter.Route(propertyName, this);
            if (route.Target == ErrorKeyTarget.Child)
            {
                foreach (var (name, child) in this.LiveChildren())
                {
                    if (string.Equals(name, route.ChildName, StringComparison.Ordinal))
                    {
                        child.SetError(route.Key, message);
                        break;
                    }
                }

                return;
            }

            if (route.Target == ErrorKeyTarget.Form)
            {
                lock (this.syncRoot)
                {
                    this.formErrors.RemoveAll(e => string.Equals(e.PropertyName, propertyName, StringComparison.Ordinal));
                    this.formErrors.Add(new ValidationError(propertyName, ValidationErrorCodes.Custom, message));
                }

                this.NotifyErrorsChanged();
            }
            else
            {
                var entry = this.TryGetEntry(route.Key)!;
                lock (this.syncRoot)
                {
                    entry.SetErrors.Clear();
                    entry.SetErrors.Add(new ValidationError(route.Key, ValidationErrorCodes.Custom, message));
                }

                this.PublishField(entry);
            }

            this.RefreshDerivedState();
        }

        private void SetErrorsCore(IEnumerable<ValidationError> errors)
        {
            var own = new Dictionary<string, List<ValidationError>>(StringComparer.Ordinal);
            var perChild = new Dictionary<string, List<ValidationError>>(StringComparer.Ordinal);
            var form = new List<ValidationError>();

            foreach (var error in errors)
            {
                if (error == null)
                {
                    continue;
                }

                var key = error.PropertyName ?? string.Empty;
                var route = key.Length == 0 ? ErrorKeyRoute.Form(string.Empty) : ErrorKeyRouter.Route(key, this);
                switch (route.Target)
                {
                    case ErrorKeyTarget.Own:
                        ListOf(own, route.Key).Add(this.WithMessage(error with { PropertyName = route.Key }));
                        break;

                    case ErrorKeyTarget.Child:
                        // The child fills in a missing message, with its own name for the field.
                        ListOf(perChild, route.ChildName!).Add(error with { PropertyName = route.Key });
                        break;

                    default:
                        if (key.Length > 0 || !string.IsNullOrWhiteSpace(error.Message))
                        {
                            form.Add(this.WithMessage(error));
                        }

                        break;
                }
            }

            var changed = new List<FieldEntry>();

            lock (this.syncRoot)
            {
                foreach (var entry in this.fieldOrder)
                {
                    var had = entry.SetErrors.Count > 0;
                    entry.SetErrors.Clear();

                    if (own.TryGetValue(entry.State.PropertyName, out var routed))
                    {
                        entry.SetErrors.AddRange(routed);
                    }

                    if (had || entry.SetErrors.Count > 0)
                    {
                        changed.Add(entry);
                    }
                }

                this.formErrors.Clear();
                this.formErrors.AddRange(form);
            }

            foreach (var (name, child) in this.LiveChildren())
            {
                child.SetErrorsCore(perChild.TryGetValue(name, out var routed) ? routed : Enumerable.Empty<ValidationError>());
            }

            foreach (var entry in changed)
            {
                this.PublishField(entry, notify: false);
            }

            this.NotifyErrorsChanged();
            this.RefreshDerivedState();

            static List<ValidationError> ListOf(Dictionary<string, List<ValidationError>> map, string key)
            {
                if (!map.TryGetValue(key, out var list))
                {
                    map[key] = list = new List<ValidationError>();
                }

                return list;
            }
        }

        /// <summary>
        /// An error without a message — a backend which names the field but sends no text — gets the default message
        /// of its code, so that a failed result never shows nothing.
        /// </summary>
        private ValidationError WithMessage(ValidationError error)
        {
            if (!string.IsNullOrWhiteSpace(error.Message))
            {
                return error;
            }

            var arguments = new Dictionary<string, object?>(error.Arguments ?? ValidationError.EmptyArguments, StringComparer.Ordinal)
            {
                ["PropertyName"] = this.DisplayNameOf(error.PropertyName),
            };

            return error with { Message = this.MessageProvider.GetMessage(error.ErrorCode, arguments), Arguments = arguments };
        }

        private void DropSetErrors(FieldEntry entry)
        {
            bool dropped;
            lock (this.syncRoot)
            {
                dropped = entry.SetErrors.Count > 0;
                entry.SetErrors.Clear();
            }

            if (dropped)
            {
                this.PublishField(entry);
            }
        }

        private void ClearSetErrorsInTree()
        {
            var changed = new List<FieldEntry>();
            bool hadFormErrors;

            lock (this.syncRoot)
            {
                foreach (var entry in this.fieldOrder)
                {
                    if (entry.SetErrors.Count > 0)
                    {
                        entry.SetErrors.Clear();
                        changed.Add(entry);
                    }
                }

                hadFormErrors = this.formErrors.Count > 0;
                this.formErrors.Clear();
            }

            foreach (var (_, child) in this.LiveChildren())
            {
                child.ClearSetErrorsInTree();
            }

            foreach (var entry in changed)
            {
                this.PublishField(entry, notify: false);
            }

            if (changed.Count > 0 || hadFormErrors)
            {
                this.NotifyErrorsChanged();
            }

            this.RefreshDerivedState();
        }

        private void DropSetErrorsChangedDuringAction()
        {
            string[] changedNames;
            lock (this.syncRoot)
            {
                changedNames = this.changedDuringAction.ToArray();
            }

            foreach (var propertyName in changedNames)
            {
                if (this.TryGetEntry(propertyName) is { } entry)
                {
                    this.DropSetErrors(entry);
                }
            }

            foreach (var (_, child) in this.LiveChildren())
            {
                child.DropSetErrorsChangedDuringAction();
            }

            this.RefreshDerivedState();
        }

        private void ForgetChangesDuringAction()
        {
            lock (this.syncRoot)
            {
                this.changedDuringAction.Clear();
            }

            foreach (var (_, child) in this.LiveChildrenForCleanUp())
            {
                child.ForgetChangesDuringAction();
            }
        }

        #endregion

        #region Reading errors

        /// <summary>
        /// Every property with at least one message, own fields first, then form-level under an empty key, then the
        /// children's prefixed with their name (<c>Address.Street</c>). Entries without messages are omitted.
        /// </summary>
        public Dictionary<string, List<string>> GetErrors()
        {
            var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            lock (this.syncRoot)
            {
                foreach (var entry in this.fieldOrder)
                {
                    var messages = MessagesOf(entry);
                    if (messages.Count > 0)
                    {
                        result[entry.State.PropertyName] = messages.ToList();
                    }
                }

                if (this.formErrors.Count > 0)
                {
                    result[string.Empty] = this.formErrors.Select(e => e.Message).ToList();
                }
            }

            foreach (var (name, child) in this.LiveChildren())
            {
                foreach (var (propertyName, messages) in child.GetErrors())
                {
                    result[$"{name}.{propertyName}"] = messages;
                }
            }

            return result;
        }

        /// <summary>
        /// <see cref="INotifyDataErrorInfo.GetErrors"/>: the messages of one field, or the form-level messages for
        /// <c>null</c> or an empty name, as a snapshot.
        /// </summary>
        public IEnumerable GetErrors(string? propertyName)
        {
            return string.IsNullOrEmpty(propertyName) ? this.FormMessages() : this.MessagesOf(propertyName);
        }

        #endregion

        #region Snapshots for Errors and Fields

        private IReadOnlyList<string> MessagesOf(string propertyName)
        {
            lock (this.syncRoot)
            {
                return this.fields.TryGetValue(propertyName, out var entry) ? MessagesOf(entry) : NoMessages;
            }
        }

        private static IReadOnlyList<string> MessagesOf(FieldEntry entry)
        {
            if (entry.RuleErrors.Count == 0 && entry.SetErrors.Count == 0)
            {
                return NoMessages;
            }

            return entry.RuleErrors.Select(pair => pair.Error.Message)
                .Concat(entry.SetErrors.Select(e => e.Message))
                .ToArray();
        }

        private static IReadOnlyList<ValidationError> ErrorsOf(FieldEntry entry)
        {
            if (entry.RuleErrors.Count == 0 && entry.SetErrors.Count == 0)
            {
                return Array.Empty<ValidationError>();
            }

            return entry.RuleErrors.Select(pair => pair.Error).Concat(entry.SetErrors).ToArray();
        }

        private IReadOnlyList<string> FormMessages()
        {
            lock (this.syncRoot)
            {
                return this.formErrors.Count == 0 ? NoMessages : this.formErrors.Select(e => e.Message).ToArray();
            }
        }

        private IReadOnlyList<KeyValuePair<string, IReadOnlyList<string>>> AllByProperty()
        {
            var result = new List<KeyValuePair<string, IReadOnlyList<string>>>();

            lock (this.syncRoot)
            {
                foreach (var entry in this.fieldOrder)
                {
                    var messages = MessagesOf(entry);
                    if (messages.Count > 0)
                    {
                        result.Add(new KeyValuePair<string, IReadOnlyList<string>>(entry.State.PropertyName, messages));
                    }
                }

                if (this.formErrors.Count > 0)
                {
                    result.Add(new KeyValuePair<string, IReadOnlyList<string>>(string.Empty, this.formErrors.Select(e => e.Message).ToArray()));
                }
            }

            foreach (var (name, child) in this.LiveChildren())
            {
                foreach (var (propertyName, messages) in child.AllByProperty())
                {
                    result.Add(new KeyValuePair<string, IReadOnlyList<string>>($"{name}.{propertyName}", messages));
                }
            }

            return result;
        }

        private IReadOnlyList<string> AllMessages()
        {
            return this.AllByProperty().SelectMany(pair => pair.Value).ToArray();
        }

        /// <summary>
        /// The state of a field with rules, or — for a name without — a detached state which takes no part in routing
        /// or focus. Should rules be declared for that name later, the field takes the detached state over.
        /// </summary>
        private FieldState FieldOf(string propertyName)
        {
            lock (this.syncRoot)
            {
                if (this.fields.TryGetValue(propertyName, out var entry))
                {
                    return entry.State;
                }

                if (!this.detachedFields.TryGetValue(propertyName, out var state))
                {
                    this.detachedFields[propertyName] = state = new FieldState(propertyName);
                }

                return state;
            }
        }

        private IReadOnlyList<FieldState> AllFields()
        {
            lock (this.syncRoot)
            {
                return this.fieldOrder.Select(e => e.State).ToArray();
            }
        }

        /// <summary>The name a property has in messages: a <c>WithDisplayName</c> of its chain, a translation, or its words.</summary>
        private string DisplayNameOf(string propertyName)
        {
            Func<string>? declared = null;
            lock (this.syncRoot)
            {
                if (this.fields.TryGetValue(propertyName, out var entry))
                {
                    declared = entry.Chains.Select(c => c.DisplayNameProvider).FirstOrDefault(p => p != null);
                }
            }

            return declared?.Invoke() ?? this.MessageProvider.GetDisplayName(propertyName) ?? PropertyRules.Humanize(propertyName);
        }

        #endregion

        #region Policy

        private bool ShouldValidateOnChange(FieldEntry entry)
        {
            bool hasRuleErrors;
            ValidationMode? modeOverride;

            lock (this.syncRoot)
            {
                if (entry.Chains.Count == 0)
                {
                    return false;
                }

                hasRuleErrors = entry.RuleErrors.Count > 0;
                modeOverride = entry.Chains.Select(c => c.ModeOverride).FirstOrDefault(m => m != null);
            }

            if (hasRuleErrors || this.hasValidated)
            {
                return this.ReValidateMode == ReValidationMode.OnChange;
            }

            var mode = modeOverride ?? this.Mode;
            return mode is ValidationMode.OnChange or ValidationMode.All
                || (mode == ValidationMode.OnTouched && entry.State.IsTouched);
        }

        private bool ShouldValidateOnBlur(FieldEntry entry)
        {
            bool hasRuleErrors;
            ValidationMode? modeOverride;

            lock (this.syncRoot)
            {
                if (entry.Chains.Count == 0)
                {
                    return false;
                }

                hasRuleErrors = entry.RuleErrors.Count > 0;
                modeOverride = entry.Chains.Select(c => c.ModeOverride).FirstOrDefault(m => m != null);
            }

            if (hasRuleErrors || this.hasValidated)
            {
                return this.ReValidateMode == ReValidationMode.OnBlur;
            }

            var mode = modeOverride ?? this.Mode;
            return mode is ValidationMode.OnBlur or ValidationMode.OnTouched or ValidationMode.All;
        }

        private async Task ValidateOnBlurAsync(FieldEntry entry, string propertyName)
        {
            try
            {
                await this.ValidateFieldAsync(entry, immediate: true, CancellationToken.None);
                this.RefreshDerivedState();
            }
            catch (Exception ex)
            {
                this.OnValidationFailed(propertyName, ex);
            }
        }

        private IEnumerable<FieldEntry> DependentsOf(string propertyName)
        {
            lock (this.syncRoot)
            {
                return this.fieldOrder
                    .Where(e => e.Chains.Any(c => c.Dependencies.Contains(propertyName)))
                    .ToArray();
            }
        }

        /// <summary>
        /// Looks at the conditions of every conditional chain: one which turned off takes its rule errors with it at
        /// once. A field whose condition flipped either way joins <paramref name="affected"/>, as if it had changed. A
        /// condition which throws — on state the view model has not loaded yet, say — counts as unchanged here; the
        /// field's own validation reports it.
        /// </summary>
        private void ApplyConditionChanges(List<FieldEntry> affected)
        {
            foreach (var entry in this.AllEntries())
            {
                PropertyRules[] chains;
                lock (this.syncRoot)
                {
                    chains = entry.Chains.Where(c => c.HasConditions).ToArray();
                }

                var flipped = false;
                var dropped = false;
                foreach (var chain in chains)
                {
                    if (chain.TryAppliesNow() is not { } applies)
                    {
                        continue;
                    }

                    var previous = chain.LastApplies;
                    chain.LastApplies = applies;

                    if (previous == null || previous == applies)
                    {
                        continue;
                    }

                    flipped = true;
                    if (!applies)
                    {
                        lock (this.syncRoot)
                        {
                            dropped |= entry.RuleErrors.RemoveAll(pair => chain.Contains(pair.Rule)) > 0;
                        }
                    }
                }

                if (dropped)
                {
                    this.PublishField(entry);
                }

                if (flipped && !affected.Contains(entry))
                {
                    affected.Add(entry);
                }
            }
        }

        /// <summary>
        /// Asks the control of the first invalid field — here, or else in the first child with one — to take focus.
        /// With <paramref name="ruleErrorsOnly"/>, only a failed rule counts: when the rules decided the verdict, an error
        /// set from outside on another field is not the reason and must not take the focus.
        /// </summary>
        private bool RequestFocusOnFirstError(bool ruleErrorsOnly)
        {
            FieldEntry? first;
            lock (this.syncRoot)
            {
                first = this.fieldOrder.FirstOrDefault(e => e.RuleErrors.Count > 0 || (!ruleErrorsOnly && e.SetErrors.Count > 0));
            }

            if (first != null)
            {
                this.FocusRequested?.Invoke(this, new FocusRequestedEventArgs(first.State.PropertyName));
                return true;
            }

            foreach (var (_, child) in this.LiveChildren())
            {
                if (child.RequestFocusOnFirstError(ruleErrorsOnly))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Derived state and notifications

        private void PublishField(FieldEntry entry, bool notify = true)
        {
            IReadOnlyList<ValidationError> errors;
            lock (this.syncRoot)
            {
                errors = ErrorsOf(entry);
            }

            entry.State.SetErrors(errors);

            if (notify)
            {
                this.Errors.NotifyChanged(entry.State.PropertyName);
                this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(entry.State.PropertyName));
                this.UpdateHasErrors();
                this.StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Any number of errors changed: every view of them is announced again.</summary>
        private void NotifyErrorsChanged()
        {
            this.Errors.NotifyChanged(null);

            // A compiled binding to Errors[X] re-reads its whole path on this one.
            this.RaisePropertyChanged(nameof(this.Errors));
            this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(null));
            this.UpdateHasErrors();
            this.StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnChildStateChanged()
        {
            this.Errors.NotifyAggregateChanged();
            this.UpdateHasErrors();
            this.UpdateDerivedState();
            this.StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateHasErrors()
        {
            var now = this.HasErrors;
            if (this.hasErrors != now)
            {
                this.hasErrors = now;
                this.RaisePropertyChanged(nameof(this.HasErrors));
            }
        }

        private void RefreshDerivedState()
        {
            if (this.UpdateDerivedState())
            {
                this.StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Recomputes <see cref="IsValid"/>, <see cref="IsDirty"/> and <see cref="IsPending"/>, announcing each that flipped.</summary>
        private bool UpdateDerivedState()
        {
            var valid = this.ComputeIsValid();
            var dirty = this.ComputeIsDirty();
            var pending = this.ComputeIsPending();
            var changed = false;

            if (this.isValid != valid)
            {
                var known = this.isValid.HasValue;
                this.isValid = valid;

                if (known)
                {
                    this.RaisePropertyChanged(nameof(this.IsValid));
                    changed = true;
                }
            }

            if (this.isDirty != dirty)
            {
                this.isDirty = dirty;
                this.RaisePropertyChanged(nameof(this.IsDirty));
                changed = true;
            }

            if (this.isPending != pending)
            {
                this.isPending = pending;
                this.RaisePropertyChanged(nameof(this.IsPending));
                changed = true;
            }

            return changed;
        }

        private bool ComputeIsValid()
        {
            var active = this.EffectiveActiveGroups();

            foreach (var entry in this.AllEntries())
            {
                if (Volatile.Read(ref entry.PendingRuns) > 0)
                {
                    // Waiting for an asynchronous rule: not known to be valid yet.
                    return false;
                }

                PropertyRules[] chains;
                bool hasAsyncVerdict;
                object? judgedValue;
                lock (this.syncRoot)
                {
                    hasAsyncVerdict = entry.RuleErrors.Any(pair => pair.Rule.IsAsync);
                    judgedValue = entry.AsyncVerdictValue;
                    chains = entry.Chains.ToArray();
                }

                // The last known verdict of an asynchronous rule counts until it runs again — for the value it judged.
                // Once the user changed that value, the rule has not said anything yet, as if it had never run.
                if (hasAsyncVerdict && chains.Length > 0 && AreSameValue(TryGetValue(chains[0]), judgedValue))
                {
                    return false;
                }

                foreach (var chain in chains)
                {
                    try
                    {
                        if (!chain.PassesSynchronously(active))
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        // The accessor or a predicate threw — on state the view model has not loaded yet, say.
                        return false;
                    }
                }
            }

            return this.LiveChildren().All(c => c.Form.ComputeIsValid());
        }

        private bool ComputeIsDirty()
        {
            return this.AllEntries().Any(e => e.State.IsDirty) || this.LiveChildren().Any(c => c.Form.ComputeIsDirty());
        }

        private bool ComputeIsPending()
        {
            return this.AllEntries().Any(e => Volatile.Read(ref e.PendingRuns) > 0) || this.LiveChildren().Any(c => c.Form.ComputeIsPending());
        }

        private object? CurrentValue(FieldEntry entry)
        {
            PropertyRules? chain;
            lock (this.syncRoot)
            {
                chain = entry.Chains.FirstOrDefault();
            }

            return chain == null ? null : TryGetValue(chain);
        }

        private void UpdateDirty(FieldEntry entry, object? value)
        {
            bool hasInitialValue;
            object? initialValue;

            lock (this.syncRoot)
            {
                hasInitialValue = entry.HasInitialValue;
                initialValue = entry.InitialValue;
            }

            if (hasInitialValue)
            {
                entry.State.SetDirty(!AreSameValue(value, initialValue));
            }
        }

        /// <summary>
        /// Watches the field's value when it is a collection which announces its changes, so that adding or removing an
        /// item counts as a change of the property, and follows the property to a new instance. The collection reaches
        /// the form through a weak subscription: one that outlives the view model keeps nothing alive.
        /// </summary>
        private void WatchCollection(FieldEntry entry, object? value)
        {
            var collection = value as INotifyCollectionChanged;
            IDisposable? previous;
            lock (this.syncRoot)
            {
                if (ReferenceEquals(entry.WatchedCollection, collection))
                {
                    return;
                }

                previous = entry.CollectionSubscription;
                entry.WatchedCollection = collection;
                entry.CollectionSubscription = null;
            }

            previous?.Dispose();
            if (collection == null)
            {
                return;
            }

            var propertyName = entry.State.PropertyName;
            var subscription = WeakSubscription.ToCollectionChanged(
                collection,
                this,
                (form, change) => _ = form.HandlePropertyChangeAsync(propertyName));

            lock (this.syncRoot)
            {
                entry.CollectionSubscription = subscription;
            }
        }

        /// <summary>
        /// What a value is compared against later: the value itself, or for a collection which announces its changes the
        /// items it holds now, since the instance stays the same while its items change. Any other collection — a
        /// <c>byte[]</c> photo, a list — counts as its instance: copying it would cost memory for nothing, as nobody
        /// hears of its changes.
        /// </summary>
        private static object? Baseline(object? value)
        {
            return value is not string and IEnumerable items and INotifyCollectionChanged
                ? new CollectionSnapshot(items)
                : value;
        }

        /// <summary>
        /// Equal values; for text, <c>null</c> and empty are the same — a field typed into and cleared is not changed —
        /// and a collection equals a snapshot holding the same items in the same order.
        /// </summary>
        private static bool AreSameValue(object? current, object? initial)
        {
            if (initial is CollectionSnapshot snapshot)
            {
                return snapshot.Matches(current);
            }

            if (current is string or null && initial is string or null)
            {
                var currentText = (string?)current;
                var initialText = (string?)initial;
                return string.IsNullOrEmpty(currentText)
                    ? string.IsNullOrEmpty(initialText)
                    : string.Equals(currentText, initialText, StringComparison.Ordinal);
            }

            return Equals(current, initial);
        }

        private IReadOnlySet<string> EffectiveActiveGroups()
        {
            HashSet<string> result;
            lock (this.syncRoot)
            {
                result = new HashSet<string>(this.activeGroups, StringComparer.Ordinal);
            }

            if (this.Parent is { } parentForm)
            {
                result.UnionWith(parentForm.EffectiveActiveGroups());
            }

            return result;
        }

        private void SetActiveGroupsCore(IEnumerable<string> groups)
        {
            var next = new HashSet<string>(groups, StringComparer.Ordinal);
            lock (this.syncRoot)
            {
                this.activeGroups = next;
            }

            this.OnActiveGroupsChanged();
        }

        /// <summary>
        /// The effective groups changed — this form's or an ancestor's: a rule whose group is no longer active has
        /// nothing to say any more, here and in the children.
        /// </summary>
        private void OnActiveGroupsChanged()
        {
            var active = this.EffectiveActiveGroups();
            var changed = new List<FieldEntry>();

            lock (this.syncRoot)
            {
                foreach (var entry in this.fieldOrder)
                {
                    if (entry.RuleErrors.RemoveAll(pair => !pair.Rule.IsActive(active)) > 0)
                    {
                        changed.Add(entry);
                    }
                }
            }

            foreach (var entry in changed)
            {
                this.PublishField(entry, notify: false);
            }

            this.RaisePropertyChanged(nameof(this.ActiveGroups));

            foreach (var (_, child) in this.LiveChildren())
            {
                child.OnActiveGroupsChanged();
            }

            if (changed.Count > 0)
            {
                this.NotifyErrorsChanged();
            }

            this.RefreshDerivedState();
        }

        #endregion

        #region Entries and children

        private FieldEntry GetOrAddEntry(string propertyName)
        {
            if (!this.fields.TryGetValue(propertyName, out var entry))
            {
                // A state handed out before the rules existed is taken over, so that its bindings stay intact.
                var state = this.detachedFields.Remove(propertyName, out var detached) ? detached : new FieldState(propertyName);
                entry = new FieldEntry(state);
                this.fields[propertyName] = entry;
                this.fieldOrder.Add(entry);
            }

            return entry;
        }

        private FieldEntry? TryGetEntry(string propertyName)
        {
            lock (this.syncRoot)
            {
                return this.fields.TryGetValue(propertyName, out var entry) ? entry : null;
            }
        }

        private FieldEntry[] AllEntries()
        {
            lock (this.syncRoot)
            {
                return this.fieldOrder.ToArray();
            }
        }

        private FieldEntry[] EntriesWithRules()
        {
            lock (this.syncRoot)
            {
                return this.fieldOrder.Where(e => e.Chains.Count > 0).ToArray();
            }
        }

        private bool IsChildName(string propertyName)
        {
            lock (this.syncRoot)
            {
                return this.children.Any(c => string.Equals(c.Name, propertyName, StringComparison.Ordinal));
            }
        }

        /// <summary>
        /// The included forms which exist right now. Resolving one links it: it inherits this form's policy and reports
        /// its changes here. A form that would include one of its own ancestors is left out.
        /// </summary>
        private IReadOnlyList<(string Name, ViewModelValidation Form)> LiveChildren()
        {
            ChildForm[] snapshot;
            lock (this.syncRoot)
            {
                if (this.children.Count == 0)
                {
                    return Array.Empty<(string, ViewModelValidation)>();
                }

                snapshot = this.children.ToArray();
            }

            var result = new List<(string, ViewModelValidation)>(snapshot.Length);
            foreach (var child in snapshot)
            {
                var form = child.Accessor();
                if (form != null && (ReferenceEquals(form, this) || this.HasAncestor(form)))
                {
                    form = null;
                }

                this.Link(child, form);

                if (form != null)
                {
                    result.Add((child.Name, form));
                }
            }

            return result;
        }

        /// <summary>
        /// Subscribes to the form a child accessor resolved to, and makes this form its parent. Both links are weak, so
        /// that a form which outlives this one — a session's, included by a page's — never keeps it alive. A form
        /// included by two parents follows the one which linked it last, while that one lives.
        /// </summary>
        /// <summary>
        /// <see cref="LiveChildren"/> for the clean-up after a validation, which runs in <c>finally</c>: an accessor which
        /// throws counts as no child here, so that the failure already on its way to the caller stays the one it sees.
        /// </summary>
        private IReadOnlyList<(string Name, ViewModelValidation Form)> LiveChildrenForCleanUp()
        {
            try
            {
                return this.LiveChildren();
            }
            catch (Exception)
            {
                return Array.Empty<(string, ViewModelValidation)>();
            }
        }

        private void Link(ChildForm child, ViewModelValidation? form)
        {
            ViewModelValidation? previous;
            IDisposable? previousSubscription = null;
            lock (this.syncRoot)
            {
                previous = child.Linked;
                if (!ReferenceEquals(previous, form))
                {
                    child.Linked = form;
                    previousSubscription = child.Subscription;
                    child.Subscription = null;
                }
            }

            if (ReferenceEquals(previous, form))
            {
                // Linked already; should the parent which took the form over since be gone, it follows this one again.
                if (form != null && form.Parent == null)
                {
                    form.SetParent(this);
                }

                return;
            }

            previousSubscription?.Dispose();
            if (previous != null && ReferenceEquals(previous.Parent, this))
            {
                previous.SetParent(null);
            }

            if (form != null)
            {
                form.SetParent(this);
                var subscription = WeakSubscription.ToEvent<ViewModelValidation, EventArgs>(
                    handler => form.StateChanged += handler,
                    handler => form.StateChanged -= handler,
                    this,
                    static (parent, _) => parent.OnChildStateChanged());

                lock (this.syncRoot)
                {
                    child.Subscription = subscription;
                }
            }
        }

        private void SetParent(ViewModelValidation? form)
        {
            Volatile.Write(ref this.parent, form == null ? null : new WeakReference<ViewModelValidation>(form));
        }

        private bool HasAncestor(ViewModelValidation form)
        {
            for (var ancestor = this.Parent; ancestor != null; ancestor = ancestor.Parent)
            {
                if (ReferenceEquals(ancestor, form))
                {
                    return true;
                }
            }

            return false;
        }

        private static object? TryGetValue(PropertyRules chain)
        {
            try
            {
                return chain.GetValue();
            }
            catch
            {
                // The accessor may read state the view model has not loaded yet; the value is then unknown.
                return null;
            }
        }

        string? IErrorKeyRoutingSource.FindFieldByAlias(string key)
        {
            (string Name, PropertyRules[] Chains)[] snapshot;
            lock (this.syncRoot)
            {
                snapshot = this.fieldOrder.Select(e => (e.State.PropertyName, e.Chains.ToArray())).ToArray();
            }

            // Alias predicates are the app's code: they run outside the lock.
            return snapshot.FirstOrDefault(field => field.Chains.Any(c => c.MatchesAlias(key))).Name;
        }

        string? IErrorKeyRoutingSource.FindField(string key)
        {
            lock (this.syncRoot)
            {
                return this.fieldOrder.FirstOrDefault(e => string.Equals(e.State.PropertyName, key, StringComparison.OrdinalIgnoreCase))?.State.PropertyName;
            }
        }

        IReadOnlyList<KeyValuePair<string, IErrorKeyRoutingSource>> IErrorKeyRoutingSource.Children
        {
            get => this.LiveChildren().Select(c => new KeyValuePair<string, IErrorKeyRoutingSource>(c.Name, c.Form)).ToArray();
        }

        private sealed class FieldEntry
        {
            public FieldEntry(FieldState state)
            {
                this.State = state;
            }

            public FieldState State { get; }

            public List<PropertyRules> Chains { get; } = new List<PropertyRules>();

            public List<(ValidationError Error, IPropertyRule Rule)> RuleErrors { get; } = new List<(ValidationError, IPropertyRule)>();

            public List<ValidationError> SetErrors { get; } = new List<ValidationError>();

            public object? InitialValue { get; set; }

            public bool HasInitialValue { get; set; }

            public RunSequencer? Sequencer { get; set; }

            /// <summary>The value the asynchronous rules last judged; their errors count for <see cref="IsValid"/> while it stays.</summary>
            public object? AsyncVerdictValue { get; set; }

            /// <summary>The explicit run in flight, which another explicit validation of the same value joins.</summary>
            public ExplicitRun? ExplicitRun { get; set; }

            /// <summary>The collection the field holds, when it announces its changes.</summary>
            public INotifyCollectionChanged? WatchedCollection { get; set; }

            /// <summary>The weak subscription to <see cref="WatchedCollection"/>'s changes.</summary>
            public IDisposable? CollectionSubscription { get; set; }

            public int PendingRuns;
        }

        private sealed class ChildForm
        {
            public ChildForm(string name, Func<ViewModelValidation?> accessor)
            {
                this.Name = name;
                this.Accessor = accessor;
            }

            public string Name { get; }

            public Func<ViewModelValidation?> Accessor { get; }

            /// <summary>The form this parent subscribed to and passes its policy to, if any.</summary>
            public ViewModelValidation? Linked { get; set; }

            /// <summary>The weak subscription to <see cref="Linked"/>'s state changes.</summary>
            public IDisposable? Subscription { get; set; }
        }

        /// <summary>What a run of one field's rules came to.</summary>
        private enum FieldVerdict
        {
            Passed,
            Failed,

            /// <summary>Replaced by a later run, or cancelled, before it judged: it says nothing about the field.</summary>
            Abandoned,
        }

        /// <summary>An explicit run in flight: the value it judges, under which groups, and the token which cancels it.</summary>
        private sealed record ExplicitRun(Task<FieldVerdict> Task, object? Value, IReadOnlySet<string> Groups, CancellationToken RunToken);

        /// <summary>
        /// One explicit validation while it runs its rules: what changed under it anywhere in its tree, and so has to
        /// be judged again before its verdict. Each validation keeps its own, so that two running at once over
        /// overlapping forms — a child's own and its parent's — both see every change.
        /// </summary>
        private sealed class RulePhase
        {
            private readonly Lock syncRoot = new Lock();
            private readonly HashSet<(ViewModelValidation Form, string Name)> fields = new HashSet<(ViewModelValidation Form, string Name)>();
            private readonly HashSet<(ViewModelValidation Form, string Name)> children = new HashSet<(ViewModelValidation Form, string Name)>();

            public bool HasChanges
            {
                get
                {
                    lock (this.syncRoot)
                    {
                        return this.fields.Count > 0 || this.children.Count > 0;
                    }
                }
            }

            public void FieldChanged(ViewModelValidation form, string name)
            {
                lock (this.syncRoot)
                {
                    this.fields.Add((form, name));
                }
            }

            public void ChildReplaced(ViewModelValidation form, string name)
            {
                lock (this.syncRoot)
                {
                    this.children.Add((form, name));
                }
            }

            /// <summary>Takes the changes recorded so far, leaving none behind.</summary>
            public ((ViewModelValidation Form, string Name)[] Fields, (ViewModelValidation Form, string Name)[] Children) Take()
            {
                lock (this.syncRoot)
                {
                    var taken = (this.fields.ToArray(), this.children.ToArray());
                    this.fields.Clear();
                    this.children.Clear();
                    return taken;
                }
            }
        }

        /// <summary>
        /// The items of a collection at one moment. A collection which cannot be read right now — another thread is
        /// changing it — yields a snapshot which matches nothing: the value counts as changed.
        /// </summary>
        private sealed class CollectionSnapshot
        {
            private readonly object?[]? items;

            public CollectionSnapshot(IEnumerable items)
            {
                try
                {
                    this.items = items.Cast<object?>().ToArray();
                }
                catch (InvalidOperationException)
                {
                    this.items = null;
                }
            }

            public bool Matches(object? value)
            {
                try
                {
                    return this.items != null && value is IEnumerable current && current.Cast<object?>().SequenceEqual(this.items);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
        }

        private sealed class ValidationGroupBuilder : IValidationGroupBuilder
        {
            private readonly ViewModelValidation form;
            private readonly string group;

            public ValidationGroupBuilder(ViewModelValidation form, string group)
            {
                this.form = form;
                this.group = group;
            }

            public PropertyRuleBuilder<TProperty> Property<TProperty>(Func<TProperty> accessor, [CallerArgumentExpression(nameof(accessor))] string? accessorExpression = null)
            {
                return this.form.PropertyCore(PropertyNameParser.Parse(accessorExpression), accessor, this.group);
            }

            public PropertyRuleBuilder<TProperty> Property<TProperty>(string propertyName, Func<TProperty> accessor)
            {
                return this.form.PropertyCore(propertyName, accessor, this.group);
            }
        }

        #endregion
    }
}
