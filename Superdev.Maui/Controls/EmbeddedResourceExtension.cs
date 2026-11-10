using System.Reflection;

namespace Superdev.Maui.Controls
{
    [ContentProperty(nameof(Name))]
    [RequireService([typeof(IServiceProvider), typeof(IProvideValueTarget)])]
    public class EmbeddedResourceExtension : IMarkupExtension
    {
        private static Assembly? Assembly;
        private string? name;

        static EmbeddedResourceExtension()
        {
            Assembly = Assembly.GetCallingAssembly();
        }

        public static void Init(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            Assembly = assembly;
        }

        public string? Name
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
                return null!;
            }

            if (Assembly == null)
            {
                return null!;
            }

            var valueTargetProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            Type? targetType = null;
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
                    if (targetType == typeof(Stream))
                    {
                        return Assembly.GetManifestResourceStream(name)!;
                    }

                    if (targetType == typeof(ImageSource))
                    {
                        // The image source may invoke the factory more than once, e.g. when it reloads,
                        // so every invocation opens a fresh stream rather than sharing an exhausted one.
                        return ImageSource.FromStream(() => Assembly.GetManifestResourceStream(name)!);
                    }
                }
            }

            return null!;
        }
    }
}