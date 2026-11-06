using System.ComponentModel;
using System.Globalization;

namespace Superdev.Maui.Controls.Animations
{
    /// <summary>
    /// Converts the name of an <see cref="Easing"/>, such as <c>SinOut</c> or <c>Easing.SinOut</c>,
    /// to the easing itself, so that <see cref="PopupAnimationBase.EasingIn"/> and
    /// <see cref="PopupAnimationBase.EasingOut"/> can be set from XAML.
    /// </summary>
    /// <remarks>
    /// MAUI ships a converter for <see cref="Easing"/> of its own, but the XAML compiler fails on it
    /// when the property is declared outside the framework, with "Value cannot be null. (Parameter
    /// 'type')" and no line number, which breaks the build of any consumer that sets an easing in
    /// markup. The XAML compiler does not know this converter, so it is invoked at runtime instead.
    /// </remarks>
    public class EasingTypeConverter : TypeConverter
    {
        private const string EasingPrefix = nameof(Easing) + ".";

        private static readonly IReadOnlyDictionary<string, Easing> EasingsByName =
            new Dictionary<string, Easing>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(Easing.Linear)] = Easing.Linear,
                [nameof(Easing.SinOut)] = Easing.SinOut,
                [nameof(Easing.SinIn)] = Easing.SinIn,
                [nameof(Easing.SinInOut)] = Easing.SinInOut,
                [nameof(Easing.CubicIn)] = Easing.CubicIn,
                [nameof(Easing.CubicOut)] = Easing.CubicOut,
                [nameof(Easing.CubicInOut)] = Easing.CubicInOut,
                [nameof(Easing.BounceOut)] = Easing.BounceOut,
                [nameof(Easing.BounceIn)] = Easing.BounceIn,
                [nameof(Easing.SpringIn)] = Easing.SpringIn,
                [nameof(Easing.SpringOut)] = Easing.SpringOut,
            };

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is not string text)
            {
                return base.ConvertFrom(context, culture, value)!;
            }

            var name = text.Trim();
            if (name.StartsWith(EasingPrefix, StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(EasingPrefix.Length);
            }

            if (EasingsByName.TryGetValue(name, out var easing))
            {
                return easing;
            }

            throw new InvalidOperationException(
                $"Cannot convert \"{text}\" into {typeof(Easing)}. Valid values are: {string.Join(", ", EasingsByName.Keys)}.");
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Easing easing)
            {
                foreach (var (name, knownEasing) in EasingsByName)
                {
                    if (ReferenceEquals(knownEasing, easing))
                    {
                        return name;
                    }
                }

                throw new NotSupportedException($"Only the named easings can be converted to a string, not {value}.");
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
