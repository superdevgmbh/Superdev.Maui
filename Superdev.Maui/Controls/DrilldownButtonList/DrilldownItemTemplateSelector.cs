namespace Superdev.Maui.Controls
{
    public class DrilldownItemTemplateSelector : DataTemplateSelector
    {
        public required DataTemplate DrilldownButtonTemplate { get; set; }

        public required DataTemplate DrilldownSwitchTemplate { get; set; }

        public required DataTemplate CustomDrilldownTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is IDrilldownButtonView)
            {
                return this.DrilldownButtonTemplate;
            }

            if (item is IDrilldownSwitchView)
            {
                return this.DrilldownSwitchTemplate;
            }

            return this.CustomDrilldownTemplate;
        }
    }
}