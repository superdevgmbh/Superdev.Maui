using System.Runtime.CompilerServices;

namespace Superdev.Maui.Validation
{
    /// <summary>
    /// A named set of rules that runs only when selected — for a view model that serves more than one purpose:
    /// <i>save draft</i> versus <i>publish</i>, <i>edit</i> versus <i>create</i>. Rules outside any group always run.
    /// </summary>
    public sealed class ValidationGroup
    {
        private readonly List<string> propertyNames = new List<string>();

        internal ValidationGroup(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        /// <summary>The properties which have at least one rule in this group, in declaration order.</summary>
        public IReadOnlyList<string> PropertyNames => this.propertyNames;

        internal void Register(string propertyName)
        {
            if (!this.propertyNames.Contains(propertyName, StringComparer.Ordinal))
            {
                this.propertyNames.Add(propertyName);
            }
        }
    }

    /// <summary>
    /// Declares rules inside a <see cref="ViewModelValidation.Group"/> block: every rule declared through it belongs to the group.
    /// </summary>
    public interface IValidationGroupBuilder
    {
        PropertyRuleBuilder<TProperty> Property<TProperty>(
            Func<TProperty> accessor,
            [CallerArgumentExpression(nameof(accessor))] string? accessorExpression = null);

        PropertyRuleBuilder<TProperty> Property<TProperty>(string propertyName, Func<TProperty> accessor);
    }
}
