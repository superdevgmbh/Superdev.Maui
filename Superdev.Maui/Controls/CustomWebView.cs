using System.Windows.Input;

namespace Superdev.Maui.Controls
{
    public class CustomWebView : WebView, IDisposable
    {
        public CustomWebView()
        {
            this.Navigating += this.OnNavigating;
            this.Navigated += this.OnNavigated;
        }

        private void OnNavigating(object sender, WebNavigatingEventArgs args)
        {
            if (this.NavigatingCommand is ICommand command)
            {
                if (command.CanExecute(args))
                {
                    command.Execute(args);
                }
            }
        }

        private void OnNavigated(object sender, WebNavigatedEventArgs args)
        {
            if (this.NavigatedCommand is ICommand command)
            {
                if (command.CanExecute(args))
                {
                    command.Execute(args);
                }
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (this.BindingContext != null)
            {
                this.EvaluateJavascript = this.EvaluateJavaScriptAsync;
            }
            else
            {
                this.EvaluateJavascript = null;
            }
        }

        public static BindableProperty EvaluateJavascriptProperty = BindableProperty.Create(
            nameof(EvaluateJavascript),
            typeof(Func<string, Task<string>>),
            typeof(CustomWebView),
            defaultBindingMode: BindingMode.OneWayToSource);

        public Func<string, Task<string>> EvaluateJavascript
        {
            get => (Func<string, Task<string>>)this.GetValue(EvaluateJavascriptProperty);
            set => this.SetValue(EvaluateJavascriptProperty, value);
        }

        public static readonly BindableProperty HeadersProperty = BindableProperty.Create(
            nameof(Headers),
            typeof(IDictionary<string, string>),
            typeof(CustomWebView));

        public IDictionary<string, string> Headers
        {
            get => (IDictionary<string, string>)this.GetValue(HeadersProperty);
            set => this.SetValue(HeadersProperty, value);
        }


        public static readonly BindableProperty NavigatingCommandProperty = BindableProperty.Create(
            nameof(NavigatingCommand),
            typeof(ICommand),
            typeof(CustomWebView));

        public ICommand NavigatingCommand
        {
            get => (ICommand)this.GetValue(NavigatingCommandProperty);
            set => this.SetValue(NavigatingCommandProperty, value);
        }

        public static readonly BindableProperty NavigatedCommandProperty = BindableProperty.Create(
            nameof(NavigatedCommand),
            typeof(ICommand),
            typeof(CustomWebView));

        public ICommand NavigatedCommand
        {
            get => (ICommand)this.GetValue(NavigatedCommandProperty);
            set => this.SetValue(NavigatedCommandProperty, value);
        }

        public void Dispose()
        {
            this.Navigating -= this.OnNavigating;
            this.Navigated -= this.OnNavigated;
        }
    }
}