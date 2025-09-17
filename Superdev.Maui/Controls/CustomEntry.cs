namespace Superdev.Maui.Controls
{
    public class CustomEntry : Entry
    {
        public static readonly BindableProperty TextContentTypeProperty =
            BindableProperty.Create(
                nameof(TextContentType),
                typeof(TextContentType),
                typeof(CustomEntry),
                TextContentType.Default);

        public TextContentType TextContentType
        {
            get => (TextContentType)this.GetValue(TextContentTypeProperty);
            set => this.SetValue(TextContentTypeProperty, value);
        }
    }
}