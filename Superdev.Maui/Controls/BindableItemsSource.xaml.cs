using System.Collections;
using System.Globalization;

namespace Superdev.Maui.Controls
{
    public partial class BindableItemsSource : ContentView
    {
        public BindableItemsSource()
        {
            this.InitializeComponent();
        }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(BindableItemsSource));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(BindableItemsSource),
                null);

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)this.GetValue(ItemTemplateProperty);
            set => this.SetValue(ItemTemplateProperty, value);
        }

        public static readonly BindableProperty SeparatorTemplateProperty =
            BindableProperty.Create(
                nameof(SeparatorTemplate),
                typeof(DataTemplate),
                typeof(BindableItemsSource),
                null);

        public DataTemplate SeparatorTemplate
        {
            get => (DataTemplate)this.GetValue(SeparatorTemplateProperty);
            set => this.SetValue(SeparatorTemplateProperty, value);
        }

        public static readonly BindableProperty SpacingProperty =
            BindableProperty.Create(
                nameof(Spacing),
                typeof(GridLength),
                typeof(BindableItemsSource),
                GridLength.Auto);

        [System.ComponentModel.TypeConverter(typeof(GridLengthTypeConverter))]
        public GridLength Spacing
        {
            get => (GridLength)this.GetValue(SpacingProperty);
            set => this.SetValue(SpacingProperty, value);
        }
    }
}