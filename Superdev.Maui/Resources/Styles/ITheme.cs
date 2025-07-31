namespace Superdev.Maui.Resources.Styles
{
    public interface ITheme
    {
        ColorConfiguration ColorConfiguration { get; set; }

        ISpacingConfiguration SpacingConfiguration { get; set; }

        IFontConfiguration FontConfiguration { get; set; }
    }
}