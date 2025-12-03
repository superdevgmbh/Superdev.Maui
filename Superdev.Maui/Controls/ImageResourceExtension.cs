namespace Superdev.Maui.Controls
{
    [ContentProperty(nameof(Source))]
    [AcceptEmptyServiceProvider]
    public class ImageResourceExtension : IMarkupExtension
    {
        public string? Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.Source == null)
            {
                return null!;
            }

            var imageSource = ImageSource.FromFile(this.Source);
            return imageSource;
        }
    }
}