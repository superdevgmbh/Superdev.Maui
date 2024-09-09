using System.Reflection;

namespace Superdev.Maui.Controls
{
    [ContentProperty("Source")]
    public class ImageResourceExtension : IMarkupExtension
    {
        public string Source { get; set; }

        private static Assembly TargetAssembly;

        /// <summary>
        /// Use <seealso cref="Init"/> to define which assembly contains the resources which are resolved using <seealso cref="ImageResourceExtension"/>.
        /// </summary>
        /// <param name="resourceAssembly"></param>
        public static void Init(Assembly resourceAssembly)
        {
            TargetAssembly = resourceAssembly;
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.Source == null)
            {
                return null;
            }

            var imageSource = ImageSource.FromResource(this.Source, TargetAssembly);
            return imageSource;
        }
    }
}