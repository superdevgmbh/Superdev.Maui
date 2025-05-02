namespace Superdev.Maui.Styles
{
    public interface ITheme
    {
        IColorConfiguration ColorConfiguration { get; set; }

        ISpacingConfiguration SpacingConfiguration { get; set; }

        IFontConfiguration FontConfiguration { get; set; }
    }
}