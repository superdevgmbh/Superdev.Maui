using System.Collections;
using Superdev.Maui.Mvvm;

namespace Superdev.Maui.Validation
{
    /// <summary>
    /// The messages of a form, shaped for XAML: <c>Validation.Errors[UserName]</c> for one field,
    /// <see cref="Form"/> for what no field owns, <see cref="All"/> for a summary. Every read is a snapshot.
    /// </summary>
    /// <remarks>
    /// A change to one field notifies <c>Item[UserName]</c>, <see cref="All"/> and <see cref="Count"/> only, so that a
    /// keystroke re-reads one binding rather than every <c>Errors[X]</c> on the page. A summary binds <see cref="All"/>.
    /// </remarks>
    public sealed class ValidationErrors : BindableBase, IReadOnlyCollection<KeyValuePair<string, IReadOnlyList<string>>>
    {
        /// <summary>The name a binding engine takes for "every indexer value changed".</summary>
        private const string AllIndexers = "Item";

        private static readonly IReadOnlyList<string> NoMessages = Array.Empty<string>();

        private readonly Func<string, IReadOnlyList<string>> messagesOf;
        private readonly Func<IReadOnlyList<string>> formMessages;
        private readonly Func<IReadOnlyList<KeyValuePair<string, IReadOnlyList<string>>>> allByProperty;
        private readonly Func<IReadOnlyList<string>> allMessages;

        internal ValidationErrors(
            Func<string, IReadOnlyList<string>> messagesOf,
            Func<IReadOnlyList<string>> formMessages,
            Func<IReadOnlyList<KeyValuePair<string, IReadOnlyList<string>>>> allByProperty,
            Func<IReadOnlyList<string>> allMessages)
        {
            this.messagesOf = messagesOf;
            this.formMessages = formMessages;
            this.allByProperty = allByProperty;
            this.allMessages = allMessages;
        }

        /// <summary>The messages of one field of this form. An unknown name yields an empty list.</summary>
        public IReadOnlyList<string> this[string propertyName]
        {
            get => propertyName == null ? NoMessages : this.messagesOf(propertyName);
        }

        /// <summary>Messages no field owns — a form-level verdict, or an incoming key nobody claimed.</summary>
        public IReadOnlyList<string> Form => this.formMessages();

        /// <summary>Every message of the form, its fields and its children, flattened — what a summary box shows.</summary>
        public IReadOnlyList<string> All => this.allMessages();

        /// <summary>The number of properties, form-level included, which have at least one message.</summary>
        public int Count => this.allByProperty().Count;

        public IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> GetEnumerator()
        {
            return this.allByProperty().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>One field changed, or — for <c>null</c> — any number of them and the form-level messages.</summary>
        internal void NotifyChanged(string? propertyName)
        {
            if (propertyName == null)
            {
                this.RaisePropertyChanged(AllIndexers);
                this.RaisePropertyChanged(nameof(this.Form));
            }
            else
            {
                this.RaisePropertyChanged($"{AllIndexers}[{propertyName}]");
            }

            this.NotifyAggregateChanged();
        }

        /// <summary>Something below this form changed — a child's field — so only the flattened views differ.</summary>
        internal void NotifyAggregateChanged()
        {
            this.RaisePropertyChanged(nameof(this.All));
            this.RaisePropertyChanged(nameof(this.Count));
        }
    }
}
