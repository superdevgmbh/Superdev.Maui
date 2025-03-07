using Superdev.Maui.Effects;

[assembly: ResolutionGroupName(Effects.Prefix)]

namespace Superdev.Maui.Effects
{
    [Obsolete("Migrate all effects to .NET MAUI effects")]
    public static class Effects
    {
        public const string Prefix = "Superdev.Maui";
    }
}
