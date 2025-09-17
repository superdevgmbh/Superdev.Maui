using Superdev.Maui.Behaviors;

namespace Superdev.Maui.Platforms.Behaviors
{
    public partial class ActivityIndicatorViewStylePlatformBehavior : PlatformBehavior<ActivityIndicator>
    {
        public static readonly BindableProperty ViewStyleProperty =
            BindableProperty.Create(
                nameof(ViewStyle),
                typeof(ActivityIndicatorViewStyle),
                typeof(ActivityIndicatorViewStylePlatformBehavior),
                ActivityIndicatorViewStyle.Medium);

        public ActivityIndicatorViewStyle ViewStyle
        {
            get => (ActivityIndicatorViewStyle)this.GetValue(ViewStyleProperty);
            set => this.SetValue(ViewStyleProperty, value);
        }
    }
}