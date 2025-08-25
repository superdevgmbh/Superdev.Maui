namespace Superdev.Maui.Behaviors
{
    [Flags]
    public enum TextDecorationFlags
    {
        None            = 0,
        TrimStart       = 1 << 0, // 1
        TrimEnd         = 1 << 1, // 2
        TrimWhitespaces = 1 << 2, // 4
        TrimStartEnd    = TrimStart | TrimEnd,                   // 3
        Trim             = TrimStart | TrimEnd | TrimWhitespaces // 7
    }
}