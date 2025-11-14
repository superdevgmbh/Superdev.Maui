using Superdev.Maui.Extensions;

namespace Superdev.Maui.Behaviors
{
    public class InputViewMaxLengthBehavior : BehaviorBase<VisualElement>
    {
        public static readonly BindableProperty MaxLengthProperty =
            BindableProperty.Create(
                nameof(MaxLength),
                typeof(int),
                typeof(InputViewMaxLengthBehavior),
                int.MaxValue);

        public int MaxLength
        {
            get => (int)this.GetValue(MaxLengthProperty);
            set => this.SetValue(MaxLengthProperty, value);
        }

        protected override void OnAttachedTo(VisualElement bindable)
        {
            base.OnAttachedTo(bindable);

            var inputView = bindable.AsEntry() ?? bindable.AsEditor() ?? bindable as InputView;
            if (inputView != null)
            {
                inputView.TextChanged += this.OnTextChanged;
            }
            else
            {
                throw new InvalidOperationException("bindable must inherit from type InputView");
            }
        }

        protected override void OnDetachingFrom(VisualElement bindable)
        {
            var inputView = bindable.AsEntry() ?? bindable.AsEditor() ?? bindable as InputView;
            if (inputView != null)
            {
                inputView.TextChanged -= this.OnTextChanged;
            }

            base.OnDetachingFrom(bindable);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not InputView inputView)
            {
                return;
            }

            var maxLength = this.MaxLength;
            if (e.NewTextValue != null && e.NewTextValue.Length > maxLength)
            {
                inputView.Text = e.NewTextValue[..maxLength];
            }
        }
    }
}