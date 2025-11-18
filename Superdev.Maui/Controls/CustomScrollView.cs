namespace Superdev.Maui.Controls
{
    public class CustomScrollView : ScrollView
    {
        public static readonly BindableProperty IsScrollEnabledProperty =
            BindableProperty.Create(
                nameof(IsScrollEnabled),
                typeof(bool),
                typeof(CustomScrollView),
                false);

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
                false);

        /// <summary>
        /// Enables or disables the bouncing effect.
        /// </summary>
        public bool IsBounceEnabled
        {
            get => (bool)this.GetValue(IsBounceEnabledProperty);
            set => this.SetValue(IsBounceEnabledProperty, value);
        }

        public static readonly BindableProperty IsHorizontalScrollbarVisibleProperty =
            BindableProperty.Create(
                nameof(IsHorizontalScrollbarVisible),
                typeof(bool),
                typeof(CustomScrollView),
                false);

        public bool IsHorizontalScrollbarVisible
        {
            get => (bool)this.GetValue(IsHorizontalScrollbarVisibleProperty);
            set => this.SetValue(IsHorizontalScrollbarVisibleProperty, value);
        }

        public static readonly BindableProperty IsVerticalScrollbarVisibleProperty =
            BindableProperty.Create(
                nameof(IsVerticalScrollbarVisible),
                typeof(bool),
                typeof(CustomScrollView),
                false);

        public bool IsVerticalScrollbarVisible
        {
            get => (bool)this.GetValue(IsVerticalScrollbarVisibleProperty);
            set => this.SetValue(IsVerticalScrollbarVisibleProperty, value);
        }
    }
}