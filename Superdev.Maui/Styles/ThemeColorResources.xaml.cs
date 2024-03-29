﻿namespace Superdev.Maui.Styles
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
            this[ThemeConstants.CustomButtonStyle.TextColor] = colorConfiguration.TextColor;
            this[ThemeConstants.CustomButtonStyle.BorderColorEnabled] = colorConfiguration.TextColor;
            this[ThemeConstants.CustomButtonStyle.BorderColorDisabled] = Colors.DarkGray;
            this[ThemeConstants.CustomButtonStyle.BorderColorPressed] = colorConfiguration.TextColor;
            this[ThemeConstants.CustomButtonStyle.BackgroundColorEnabled] = Colors.White;
            this[ThemeConstants.CustomButtonStyle.BackgroundColorDisabled] = colorConfiguration.TextColorBright;
            this[ThemeConstants.CustomButtonStyle.BackgroundColorPressed] = colorConfiguration.TextColorBright;

            this[ThemeConstants.CustomButtonPrimaryStyle.TextColor] = colorConfiguration.OnPrimary;
            this[ThemeConstants.CustomButtonPrimaryStyle.BorderColorEnabled] = colorConfiguration.Primary;
            this[ThemeConstants.CustomButtonPrimaryStyle.BorderColorDisabled] = colorConfiguration.PrimaryDisabled;
            this[ThemeConstants.CustomButtonPrimaryStyle.BorderColorPressed] = colorConfiguration.Primary;
            this[ThemeConstants.CustomButtonPrimaryStyle.BackgroundColorEnabled] = colorConfiguration.Primary;
            this[ThemeConstants.CustomButtonPrimaryStyle.BackgroundColorDisabled] = colorConfiguration.PrimaryDisabled;
            this[ThemeConstants.CustomButtonPrimaryStyle.BackgroundColorPressed] = colorConfiguration.PrimaryVariant;

            this[ThemeConstants.CustomButtonSecondaryStyle.TextColor] = colorConfiguration.Secondary;
            this[ThemeConstants.CustomButtonSecondaryStyle.BorderColorEnabled] = colorConfiguration.Secondary;
            this[ThemeConstants.CustomButtonSecondaryStyle.BorderColorDisabled] = colorConfiguration.SecondaryDisabled;
            this[ThemeConstants.CustomButtonSecondaryStyle.BorderColorPressed] = colorConfiguration.Secondary;
            this[ThemeConstants.CustomButtonSecondaryStyle.BackgroundColorEnabled] = colorConfiguration.OnSecondary;
            this[ThemeConstants.CustomButtonSecondaryStyle.BackgroundColorDisabled] = colorConfiguration.SecondaryDisabled;
            this[ThemeConstants.CustomButtonSecondaryStyle.BackgroundColorPressed] = colorConfiguration.SecondaryVariant;
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