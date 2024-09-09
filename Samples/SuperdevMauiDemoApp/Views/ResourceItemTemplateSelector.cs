using SuperdevMauiDemoApp.ViewModels;

namespace SuperdevMauiDemoApp.Views
{
    public class ResourceItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ColorTemplate { get; set; }

        public DataTemplate FontTemplate { get; set; }

        public DataTemplate ThicknessTemplate { get; set; }

        public DataTemplate GenericTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is ResourceViewModel resource && resource.Value != null)
            {
                var type = resource.Value.GetType();
                if (type == typeof(Color))
                {
                    return this.ColorTemplate;
                }

                if (type == typeof(Microsoft.Maui.Font))
                {
                    return this.FontTemplate;
                }

                if (type == typeof(Thickness))
                {
                    return this.ThicknessTemplate;
                }
            }

            return this.GenericTemplate;
        }
    }
}
