using System.Reflection;

namespace Superdev.Maui.Controls
{
    [ContentProperty(nameof(Name))]
    public class EmbeddedResourceExtension : IMarkupExtension
    {
        private static Assembly Assembly;
        private string name;

        static EmbeddedResourceExtension()
        {
            Assembly = Assembly.GetCallingAssembly();
        }

        public static void Init(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            Assembly = assembly;
        }

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value?.Trim()
                    .Replace('/', '.')
                    .Replace('\\', '.');
            }
        }

        public virtual object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.Name == null)
            {
                return null;
            }

            if (Assembly == null)
            {
                return null;
            }

            var valueTargetProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            Type targetType = null;
            if (valueTargetProvider?.TargetProperty is BindableProperty bindableProperty)
            {
                targetType = bindableProperty.ReturnType;
            }
            else if (valueTargetProvider?.TargetProperty is PropertyInfo propertyInfo)
            {
                targetType = propertyInfo.PropertyType;
            }

            var resourceName = this.Name;

            var manifestResourceNames = Assembly.GetManifestResourceNames();
            foreach (var name in manifestResourceNames)
            {
                if (name.EndsWith(resourceName, StringComparison.InvariantCultureIgnoreCase))
                {
                    var stream = Assembly.GetManifestResourceStream(name)!;

                    if (targetType == typeof(Stream))
                    {
                        return stream;
                    }

                    if (targetType == typeof(ImageSource))
                    {
                        return ImageSource.FromStream(() => stream);
                    }
                }
            }

            return null;
        }
    }
}