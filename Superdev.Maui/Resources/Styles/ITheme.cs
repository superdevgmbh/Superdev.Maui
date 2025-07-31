namespace Superdev.Maui.Resources.Styles
{
    public interface ITheme
    {
        ColorConfiguration ColorConfiguration { get; set; }

        SpacingConfiguration SpacingConfiguration { get; set; }

        FontConfiguration FontConfiguration { get; set; }
    }
}