namespace Superdev.Maui.Resources.Styles
{
    public partial class ThemeColorResources : ResourceDictionary
    {
        internal ThemeColorResources(IColorConfiguration colorConfiguration)
        {
            this.InitializeComponent();

            this.SetThemeColors(colorConfiguration);
            this.SetButtonColors(colorConfiguration);
            this.SetDrilldownButtonColors(colorConfiguration);
            this.SetCardViewColors(colorConfiguration);
        }

        private void SetButtonColors(IColorConfiguration colorConfiguration)
        {
            // Default button style
            this[ThemeConstants.Button.TextColor] = colorConfiguration.TextColor;
            this[ThemeConstants.Button.BorderColor] = colorConfiguration.TextColor;
            this[ThemeConstants.Button.BackgroundColor] = MaterialColors.White;

            this[ThemeConstants.Button.TextColorPressed] = MaterialColors.White;
            this[ThemeConstants.Button.BorderColorPressed] = colorConfiguration.TextColor;
            this[ThemeConstants.Button.BackgroundColorPressed] = colorConfiguration.TextColor;

            this[ThemeConstants.Button.TextColorDisabled] = colorConfiguration.PrimaryDisabled;
            this[ThemeConstants.Button.BorderColorDisabled] = colorConfiguration.PrimaryDisabled;
            this[ThemeConstants.Button.BackgroundColorDisabled] = MaterialColors.Gray200;

            // Primary button style
            this[ThemeConstants.Button.Primary.TextColor] = colorConfiguration.OnPrimary;
            this[ThemeConstants.Button.Primary.BorderColor] = colorConfiguration.Primary;
            this[ThemeConstants.Button.Primary.BackgroundColor] = colorConfiguration.Primary;

            this[ThemeConstants.Button.Primary.TextColorPressed] = colorConfiguration.OnPrimary;
            this[ThemeConstants.Button.Primary.BorderColorPressed] = colorConfiguration.Primary;
            this[ThemeConstants.Button.Primary.BackgroundColorPressed] = colorConfiguration.PrimaryVariant;

            this[ThemeConstants.Button.Primary.TextColorDisabled] = colorConfiguration.OnPrimary;
            this[ThemeConstants.Button.Primary.BorderColorDisabled] = colorConfiguration.PrimaryDisabled;
            this[ThemeConstants.Button.Primary.BackgroundColorDisabled] = colorConfiguration.PrimaryDisabled;

            // Secondary button style
            this[ThemeConstants.Button.Secondary.TextColor] = colorConfiguration.Secondary;
            this[ThemeConstants.Button.Secondary.BorderColor] = colorConfiguration.Secondary;
            this[ThemeConstants.Button.Secondary.BackgroundColor] = colorConfiguration.OnSecondary;

            this[ThemeConstants.Button.Secondary.TextColorPressed] = colorConfiguration.Secondary;
            this[ThemeConstants.Button.Secondary.BorderColorPressed] = colorConfiguration.Secondary;
            this[ThemeConstants.Button.Secondary.BackgroundColorPressed] = colorConfiguration.SecondaryVariant;

            this[ThemeConstants.Button.Secondary.TextColorDisabled] = colorConfiguration.PrimaryDisabled;
            this[ThemeConstants.Button.Secondary.BorderColorDisabled] = colorConfiguration.PrimaryDisabled;
            this[ThemeConstants.Button.Secondary.BackgroundColorDisabled] = MaterialColors.Gray200;
        }

        private void SetDrilldownButtonColors(IColorConfiguration colorConfiguration)
        {
            this[ThemeConstants.DrilldownButtonStyle.TextColor] = colorConfiguration.TextColor;
            this[ThemeConstants.DrilldownButtonStyle.BorderColorEnabled] = Colors.Transparent;
            this[ThemeConstants.DrilldownButtonStyle.BorderColorDisabled] = Colors.Transparent;
            this[ThemeConstants.DrilldownButtonStyle.BorderColorPressed] = Colors.Transparent;
            this[ThemeConstants.DrilldownButtonStyle.BackgroundColorEnabled] = Colors.Transparent;
            this[ThemeConstants.DrilldownButtonStyle.BackgroundColorDisabled] = colorConfiguration.SemiTransparentBright;
            this[ThemeConstants.DrilldownButtonStyle.BackgroundColorPressed] = colorConfiguration.SemiTransparentBright;
        }

        private void SetCardViewColors(IColorConfiguration colorConfiguration)
        {
            this[ThemeConstants.CardViewStyle.HeaderTextColor] = colorConfiguration.CardViewHeaderTextColor;
            this[ThemeConstants.CardViewStyle.HeaderBackgroundColor] = colorConfiguration.CardViewHeaderBackgroundColor;
            this[ThemeConstants.CardViewStyle.HeaderDividerColor] = colorConfiguration.CardViewDividerColor;
            this[ThemeConstants.CardViewStyle.BackgroundColor] = colorConfiguration.CardViewBackgroundColor;
            this[ThemeConstants.CardViewStyle.FooterTextColor] = colorConfiguration.CardViewFooterTextColor;
            this[ThemeConstants.CardViewStyle.FooterDividerColor] = colorConfiguration.CardViewDividerColor;
        }

        private void SetThemeColors(IColorConfiguration colorConfiguration)
        {
            this.TryAddColorResource(ThemeConstants.Color.TextColor, colorConfiguration.TextColor);
            this.TryAddColorResource(ThemeConstants.Color.TextColorBright, colorConfiguration.TextColorBright);
            this.TryAddColorResource(ThemeConstants.Color.Primary, colorConfiguration.Primary);
            this.TryAddColorResource(ThemeConstants.Color.PrimaryVariant, colorConfiguration.PrimaryVariant);
            this.TryAddColorResource(ThemeConstants.Color.PrimaryDisabled, colorConfiguration.PrimaryDisabled);
            this.TryAddColorResource(ThemeConstants.Color.OnPrimary, colorConfiguration.OnPrimary);
            this.TryAddColorResource(ThemeConstants.Color.Secondary, colorConfiguration.Secondary);
            this.TryAddColorResource(ThemeConstants.Color.SecondaryVariant, colorConfiguration.SecondaryVariant);
            this.TryAddColorResource(ThemeConstants.Color.SecondaryDisabled, colorConfiguration.SecondaryDisabled);
            this.TryAddColorResource(ThemeConstants.Color.OnSecondary, colorConfiguration.OnSecondary);
            this.TryAddColorResource(ThemeConstants.Color.BACKGROUND, colorConfiguration.Background);
            this.TryAddColorResource(ThemeConstants.Color.ON_BACKGROUND, colorConfiguration.OnBackground);
            this.TryAddColorResource(ThemeConstants.Color.SURFACE, colorConfiguration.Surface);
            this.TryAddColorResource(ThemeConstants.Color.ON_SURFACE, colorConfiguration.OnSurface);
            this.TryAddColorResource(ThemeConstants.Color.ERROR, colorConfiguration.Error);
            this.TryAddColorResource(ThemeConstants.Color.ON_ERROR, colorConfiguration.OnError);
            this.TryAddColorResource(ThemeConstants.Color.ErrorBackground, colorConfiguration.ErrorBackground);

            this.TryAddColorResource(ThemeConstants.Color.SemiTransparentBright, colorConfiguration.SemiTransparentBright);
            this.TryAddColorResource(ThemeConstants.Color.SemiTransparentDark, colorConfiguration.SemiTransparentDark);
        }
    }
}