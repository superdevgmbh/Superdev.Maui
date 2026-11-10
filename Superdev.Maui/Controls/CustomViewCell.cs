namespace Superdev.Maui.Controls
{
    /// <summary>
    /// A <see cref="ViewCell"/> whose row shows <see cref="SelectedBackgroundColor"/> while its <see cref="ListView"/> selects it.
    /// </summary>
    public class CustomViewCell : ViewCell
    {
        public static readonly BindableProperty SelectedBackgroundColorProperty = BindableProperty.Create(
            nameof(SelectedBackgroundColor),
            typeof(Color),
            typeof(CustomViewCell),
            null);

        /// <summary>
        /// The background of the row while the list selects it. On iOS, a row without it shows no selection.
        /// </summary>
        public Color SelectedBackgroundColor
        {
            get => (Color)this.GetValue(SelectedBackgroundColorProperty);
            set => this.SetValue(SelectedBackgroundColorProperty, value);
        }
    }
}