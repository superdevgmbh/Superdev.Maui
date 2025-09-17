using System.Globalization;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Localization
{
    [Preserve(AllMembers = true)]
    public class Localizer : ILocalizer
    {
        private static readonly Lazy<ILocalizer> Implementation = new Lazy<ILocalizer>(CreateLocalizer, LazyThreadSafetyMode.PublicationOnly);

        public static ILocalizer Current => Implementation.Value;

        private static ILocalizer CreateLocalizer()
        {
            return new Localizer();
        }

        private Localizer()
        {
        }

        public void SetCultureInfo(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                throw new ArgumentNullException(nameof(cultureInfo));
            }

            if (!cultureInfo.Equals(Thread.CurrentThread.CurrentCulture))
            {
                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = cultureInfo;

                this.OnLocaleChanged(cultureInfo);
            }
        }

        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        protected virtual void OnLocaleChanged(CultureInfo ci)
        {
            this.LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(ci));
        }

        public CultureInfo GetCurrentCulture()
        {
            return Thread.CurrentThread.CurrentCulture;
        }
    }
}