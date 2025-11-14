using System.Globalization;
using Superdev.Maui.Internals;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;

namespace Superdev.Maui.Localization
{
    [Preserve(AllMembers = true)]
    public class Localizer : BindableBase, ILocalizer
    {
        private readonly IMainThread mainThread;
        private static readonly Lazy<ILocalizer> Implementation = new Lazy<ILocalizer>(CreateLocalizer, LazyThreadSafetyMode.PublicationOnly);

        public static ILocalizer Current => Implementation.Value;

        private static ILocalizer CreateLocalizer()
        {
            return new Localizer(IMainThread.Current);
        }

        private CultureInfo currentCulture;

        private Localizer(IMainThread mainThread)
        {
            this.mainThread = mainThread;
            this.CurrentCulture = Thread.CurrentThread.CurrentCulture;
        }

        public CultureInfo CurrentCulture
        {
            get => this.currentCulture;
            set
            {
                if (value != null && this.SetProperty(ref this.currentCulture, value))
                {
                    this.SetCultureInfo(value);
                }
            }
        }

        private void SetCultureInfo(CultureInfo cultureInfo)
        {
            ArgumentNullException.ThrowIfNull(cultureInfo);

            if (this.mainThread.IsMainThread)
            {
                SetCultureInfoInternal();
            }
            else
            {
                this.mainThread.BeginInvokeOnMainThread(SetCultureInfoInternal);
            }

            void SetCultureInfoInternal()
            {
                Trace.WriteLine($"SetCultureInfo: cultureInfo={cultureInfo.Name}");

                this.OnLanguageChanging(cultureInfo);

                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = cultureInfo;

                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

                this.OnLanguageChanged(cultureInfo);
            }
        }

        public event EventHandler<LanguageChangingEventArgs> LanguageChanging;

        private void OnLanguageChanging(CultureInfo ci)
        {
            this.LanguageChanging?.Invoke(this, new LanguageChangingEventArgs(ci));
        }

        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        protected virtual void OnLanguageChanged(CultureInfo ci)
        {
            this.LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(ci));
        }
    }
}