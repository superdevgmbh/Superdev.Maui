
namespace Superdev.Maui.Styles
{
    public partial class ThemeSpacingResources : ResourceDictionary
    {
        public ThemeSpacingResources(ISpacingConfiguration spacingConfiguration)
        {
            this.InitializeComponent();

            this.SetSpacings(spacingConfiguration);
            this.SetPaddings(spacingConfiguration);
        }

        private void SetSpacings(ISpacingConfiguration spacingConfiguration)
        {
            this[ThemeConstants.Spacings.SmallSpacing] = spacingConfiguration.SmallSpacing;
            this[ThemeConstants.Spacings.MediumSpacing] = spacingConfiguration.MediumSpacing;
            this[ThemeConstants.Spacings.LargeSpacing] = spacingConfiguration.LargeSpacing;
            this[ThemeConstants.Spacings.CardSpacing] = spacingConfiguration.CardSpacing;
        }

        private void SetPaddings(ISpacingConfiguration spacingConfiguration)
        {
            this[ThemeConstants.Paddings.SmallPadding] = spacingConfiguration.SmallPadding;
            this[ThemeConstants.Paddings.MediumPadding] = spacingConfiguration.MediumPadding;
            this[ThemeConstants.Paddings.LargePadding] = spacingConfiguration.LargePadding;
            this[ThemeConstants.Paddings.CardViewPadding] = spacingConfiguration.CardViewPadding;
            this[ThemeConstants.Paddings.CardPadding] = spacingConfiguration.CardPadding;
        }
    }
}