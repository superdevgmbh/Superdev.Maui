using Superdev.Maui.Extensions;

namespace Superdev.Maui.Effects
{
    public class TintImageEffect : RoutingEffect
    {
        public static readonly BindableProperty TintColorProperty =
            BindableProperty.CreateAttached(
                "TintColor",
                typeof(Color),
                typeof(TintImageEffect),
                null,
                propertyChanged: OnTintColorChanged);

        public static Color GetTintColor(BindableObject bindable)
        {
            return (Color)bindable.GetValue(TintColorProperty);
        }

        public static void SetTintColor(BindableObject bindable, Color value)
        {
            bindable.SetValue(TintColorProperty, value);
        }

        public static void OnTintColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not View view)
            {
                return;
            }

            var existingEffect = view.Effects.FirstOrDefault<TintImageEffect>();

            if (newValue is Color)
            {
                if (existingEffect == null)
                {
                    view.Effects.Add(new TintImageEffect());
                }
            }
            else
            {
                if (existingEffect != null)
                {
                    view.Effects.Remove(existingEffect);
                }
            }
        }
    }
}