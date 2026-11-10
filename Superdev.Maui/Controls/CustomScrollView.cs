namespace Superdev.Maui.Controls
{
    /// <summary>
    /// A <see cref="ScrollView"/> whose scrolling, bouncing and scroll bars can be switched off.
    /// </summary>
    public class CustomScrollView : ScrollView
    {
        public static readonly BindableProperty IsScrollEnabledProperty =
            BindableProperty.Create(
                nameof(IsScrollEnabled),
                typeof(bool),
                typeof(CustomScrollView),
                true);

        /// <summary>
        /// Enables or disables scrolling by touch, in either orientation. Scrolling from code still works.
        /// Default: <c>true</c>.
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
        /// Enables or disables the bounce at the content's edges on iOS and the overscroll effect on Android.
        /// Default: <c>false</c>, unlike the platforms' own scroll views.
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