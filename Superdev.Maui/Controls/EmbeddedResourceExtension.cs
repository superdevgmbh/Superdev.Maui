using System.Reflection;

namespace Superdev.Maui.Controls
{
    [ContentProperty(nameof(Name))]
    public class EmbeddedResourceExtension : IMarkupExtension
    {
        private static Assembly Assembly;

        static EmbeddedResourceExtension()
        {
            Assembly = Assembly.GetEntryAssembly();
        }

        public static void Init(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            
            Assembly = assembly;
        }

        public string Name { get; set; }

        public virtual object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.Name == null)
            {
                return null;
            }

            var resourceName = "." + this.Name.Trim()
                .Replace('/', '.')
                .Replace('\\', '.');

            foreach (var name in Assembly.GetManifestResourceNames())
            {
                if (name.EndsWith(resourceName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return Assembly.GetManifestResourceStream(name);
                }
            }

            return null;
        }
    }
}