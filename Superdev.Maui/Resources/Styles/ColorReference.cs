using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Superdev.Maui.Resources.Styles
{
    public class ColorReference : INotifyPropertyChanged
    {
        private Color? value;

        public Color? Value
        {
            get => this.value;
            set => this.RaiseAndSetIfChanged(ref this.value, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            this.OnPropertyChanged(propertyName!);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static implicit operator Color(ColorReference colorReference) => colorReference.Value;

        public static implicit operator ColorReference(Color color) => new ColorReference { Value = color };
    }
}