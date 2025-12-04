namespace Superdev.Maui.Controls
{
    public class DrilldownCellTemplateSelector : DataTemplateSelector
    {
        public required DataTemplate DrilldownButtonCellTemplate { get; set; }

        public required DataTemplate DrilldownSwitchCellTemplate { get; set; }

        public required DataTemplate CustomDrilldownCellTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is IDrilldownButtonView)
            {
                return this.DrilldownButtonCellTemplate;
            }

            if (item is IDrilldownSwitchView)
            {
                return this.DrilldownSwitchCellTemplate;
            }

            return this.CustomDrilldownCellTemplate;
        }
    }
}