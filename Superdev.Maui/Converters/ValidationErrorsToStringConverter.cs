using System.Globalization;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Converters
{
    public class ValidationErrorsToStringConverter : BindableObject, IValueConverter
    {
        private const string DefaultBullet = "• ";

        public static readonly BindableProperty BulletProperty =
            BindableProperty.Create(
                nameof(Bullet),
                typeof(string),
                typeof(ValidationErrorsToStringConverter),
                defaultValue: DefaultBullet);

        public string Bullet
        {
            get => (string)this.GetValue(BulletProperty);
            set => this.SetValue(BulletProperty, value);
        }

        public static readonly BindableProperty ShowBulletsProperty =
            BindableProperty.Create(
                nameof(ShowBullets),
                typeof(bool),
                typeof(ValidationErrorsToStringConverter),
                defaultValue: true);

        public bool ShowBullets
        {
            get => (bool)this.GetValue(ShowBulletsProperty);
            set => this.SetValue(ShowBulletsProperty, value);
        }

        public virtual object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var messages = value switch
            {
                ViewModelValidation validation => validation.Errors.All,
                ValidationErrors errors => errors.All,
                IEnumerable<string> strings => strings.ToArray(),
                _ => Array.Empty<string>(),
            };

            if (messages.Count == 0)
            {
                return null;
            }

            var bullet = this.ShowBullets ? this.Bullet : null;
            return string.Join(Environment.NewLine, messages.Select(m => $"{bullet}{m}"));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported");
        }
    }
}