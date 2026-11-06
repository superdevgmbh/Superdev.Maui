using System.Collections;
using Superdev.Maui.Mvvm;

namespace Superdev.Maui.Validation
{
    /// <summary>
    /// The fields of a form by name: <c>Validation.Fields[UserName]</c>. A name without rules yields an inert
    /// <see cref="FieldState"/> that is not part of the form, so a binding never breaks a page and a typo never
    /// claims error keys or focus.
    /// </summary>
    public sealed class FieldStates : BindableBase, IReadOnlyCollection<FieldState>
    {
        private readonly Func<string, FieldState> resolve;
        private readonly Func<IReadOnlyList<FieldState>> all;

        internal FieldStates(Func<string, FieldState> resolve, Func<IReadOnlyList<FieldState>> all)
        {
            this.resolve = resolve;
            this.all = all;
        }

        public FieldState this[string propertyName]
        {
            get
            {
                ArgumentNullException.ThrowIfNull(propertyName);
                return this.resolve(propertyName);
            }
        }

        /// <summary>The fields which have rules, in declaration order.</summary>
        public int Count => this.all().Count;

        public IEnumerator<FieldState> GetEnumerator()
        {
            return this.all().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>A field was declared. The states themselves are stable instances which notify on their own.</summary>
        internal void NotifyFieldAdded()
        {
            this.RaisePropertyChanged(nameof(this.Count));
        }
    }
}
