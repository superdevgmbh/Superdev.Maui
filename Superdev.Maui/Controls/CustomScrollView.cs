namespace Superdev.Maui.Controls
{
    public class CustomScrollView : ScrollView
    {
        public static readonly BindableProperty IsScrollEnabledProperty =
            BindableProperty.Create(
                nameof(IsScrollEnabled),
                typeof(bool),
                typeof(CustomScrollView),
                default(bool));

        /// <summary>
        /// Enables or disables the scrollability via input gestures of this ScrollView.
        /// </summary>
        public bool IsScrollEnabled
        {
            get => (bool)this.GetValue(IsScrollEnabledProperty);
            set => this.SetValue(IsScrollEnabledProperty, value);
        }

        public static readonly BindableProperty IsBounceEnabledProperty =
            BindableProperty.Create(
                nameof(IsBounceEnabled),
                typeof(bool),
                typeof(CustomScrollView),
                default(bool));

        /// <summary>
        /// Enables or disables the bouncing effect.
        /// </summary>
        public bool IsBounceEnabled
        {
            get => (bool)this.GetValue(IsBounceEnabledProperty);
            set => this.SetValue(IsBounceEnabledProperty, value);
        }
    }
}
