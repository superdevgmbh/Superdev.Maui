using Superdev.Maui.Extensions;
using Superdev.Maui.Platforms.Behaviors;

namespace Superdev.Maui.Behaviors
{
    public class ActivityIndicatorViewStyleBehavior : Behavior<ActivityIndicator>
    {
        public static readonly BindableProperty ViewStyleProperty =
            BindableProperty.CreateAttached(
                "ViewStyle",
                typeof(ActivityIndicatorViewStyle),
                typeof(ActivityIndicatorViewStyleBehavior),
                default(ActivityIndicatorViewStyle),
                propertyChanged: OnActivityIndicatorViewStyleChanged);

        public static ActivityIndicatorViewStyle GetViewStyle(BindableObject view)
        {
            return (ActivityIndicatorViewStyle)view.GetValue(ViewStyleProperty);
        }

        public static void SetViewStyle(BindableObject view, ActivityIndicatorViewStyle value)
        {
            view.SetValue(ViewStyleProperty, value);
        }

        private static void OnActivityIndicatorViewStyleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not ActivityIndicator view)
            {
                return;
            }

            var existingBehavior = view.Behaviors.FirstOrDefault<ActivityIndicatorViewStylePlatformBehavior>();

            if (newValue is ActivityIndicatorViewStyle viewStyle)
            {
                if (existingBehavior == null)
                {
                    view.Behaviors.Add(new ActivityIndicatorViewStylePlatformBehavior { ViewStyle = viewStyle });
                }
                else
                {
                    existingBehavior.ViewStyle = viewStyle;
                }
            }
            else
            {
                if (existingBehavior != null)
                {
                    view.Behaviors.Remove(existingBehavior);
                }
            }
        }
    }
}