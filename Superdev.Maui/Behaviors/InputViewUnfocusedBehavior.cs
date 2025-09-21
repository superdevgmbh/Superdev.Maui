using Superdev.Maui.Extensions;

namespace Superdev.Maui.Behaviors
{
    /// <summary>
    /// Apply this behavior to an <seealso cref="Entry"/> in order to trim Entry's Text property if the Unfocused event is raised.
    /// </summary>
    ///
    /// <example>
    /// <Entry Placeholder="Entry 1">
    ///     <Entry.Behaviors>
    ///         <behaviors:InputViewUnfocusedBehavior DecorationFlags="All" />
    /// </Entry.Behaviors >
    /// </Entry >
    ///
    /// <Entry Placeholder="Entry 2">
    ///     <Entry.Behaviors>
    ///         <behaviors:InputViewUnfocusedBehavior DecorationFlags="Trim" />
    /// </Entry.Behaviors >
    /// </Entry >
    /// </example>
    public class InputViewUnfocusedBehavior : BehaviorBase<VisualElement>
    {
        public static readonly BindableProperty DecorationFlagsProperty =
            BindableProperty.Create(
                nameof(DecorationFlags),
                typeof(TextDecorationFlags),
                typeof(InputViewUnfocusedBehavior),
                TextDecorationFlags.None);

        public TextDecorationFlags DecorationFlags
        {
            get => (TextDecorationFlags)this.GetValue(DecorationFlagsProperty);
            set => this.SetValue(DecorationFlagsProperty, value);
        }

        protected override void OnAttachedTo(VisualElement bindable)
        {
            base.OnAttachedTo(bindable);

            var inputView = (InputView)bindable.AsEntry() ?? bindable.AsEditor() ?? bindable;
            if (inputView != null)
            {
                inputView.Unfocused += this.OnInputViewUnfocused;
            }
            else
            {
                throw new InvalidOperationException("bindable must inherit from type InputView");
            }
        }

        protected override void OnDetachingFrom(VisualElement bindable)
        {
            var inputView = (InputView)bindable.AsEntry() ?? bindable.AsEditor() ?? bindable;
            inputView.Unfocused -= this.OnInputViewUnfocused;

            base.OnDetachingFrom(bindable);
        }

        private void OnInputViewUnfocused(object sender, EventArgs e)
        {
            if (sender is not InputView inputView)
            {
                return;
            }

            if (inputView.Text is string value &&
                this.DecorationFlags is var flags && flags != TextDecorationFlags.None)
            {
                if (flags.HasFlag(TextDecorationFlags.TrimWhitespaces))
                {
                    value = value.TrimWhitespaces();
                }

                if (flags.HasFlag(TextDecorationFlags.TrimStart))
                {
                    value = value.TrimStart(StringExtensions.TrimChars);
                }

                if (flags.HasFlag(TextDecorationFlags.TrimEnd))
                {
                    value = value.TrimEnd(StringExtensions.TrimChars);
                }

                inputView.Text = value;
            }
        }
    }
}
