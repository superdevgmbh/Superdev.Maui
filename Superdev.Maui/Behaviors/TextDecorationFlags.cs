namespace Superdev.Maui.Behaviors
{
    [Flags]
    public enum TextDecorationFlags
    {
        None = 0,
        TrimStart = 1,
        TrimEnd = 2,
        Trim = TrimStart | TrimEnd,
        TrimWhitespaces = 4,
        All = Trim | TrimWhitespaces,
    }
}