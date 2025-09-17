using System.ComponentModel;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Localization
{
    [Preserve(AllMembers = true)]
    public class TranslationData : INotifyPropertyChanged, IDisposable
    {
        private readonly string key;
        private readonly ILocalizer localizer;
        private readonly ITranslationProvider translationProvider;

        public TranslationData(string key, ILocalizer localizer, ITranslationProvider translationProvider)
        {
            this.key = key;
            this.localizer = localizer;
            this.translationProvider = translationProvider;

            this.localizer.LanguageChanged += this.OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
        }

        public object Value
        {
            get
            {
                return this.translationProvider.Translate(this.key);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        ~TranslationData()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.localizer.LanguageChanged -= this.OnLanguageChanged;
            }
        }
    }
}