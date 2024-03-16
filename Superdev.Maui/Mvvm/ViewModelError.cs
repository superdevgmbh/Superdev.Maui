using System.Windows.Input;

namespace Superdev.Maui.Mvvm
{
    public class ViewModelError : BindableBase
    {
        public static readonly ViewModelError None = new ViewModelError(text: null, command: null);

        public ViewModelError(string text, Action action)
            : this(text, new Command(action))
        {
        }

        public ViewModelError(string text, ICommand command)
        {
            this.Text = text;
            this.Command = command;
        }

        public string Text { get; }

        public ICommand Command { get; }

        public bool HasError => this.Text != null;
    }
}