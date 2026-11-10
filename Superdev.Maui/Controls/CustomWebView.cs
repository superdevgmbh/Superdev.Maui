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

        private void OnNavigating(object? sender, WebNavigatingEventArgs args)
        {
            if (this.NavigatingCommand is ICommand command)
            {
                if (command.CanExecute(args))
                {
                    command.Execute(args);
                }
            }
        }

        private void OnNavigated(object? sender, WebNavigatedEventArgs args)
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

        public static readonly BindableProperty EvaluateJavascriptProperty = BindableProperty.Create(
            nameof(EvaluateJavascript),
            typeof(Func<string, Task<string>>),
            typeof(CustomWebView),
            defaultBindingMode: BindingMode.OneWayToSource);

        public Func<string, Task<string>>? EvaluateJavascript
        {
            get => (Func<string, Task<string>>?)this.GetValue(EvaluateJavascriptProperty);
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
        
        /// <summary>
        /// Allows a page loaded over HTTPS to load resources over HTTP. Off by default, which is the
        /// platform default as well.
        /// </summary>
        /// <remarks>
        /// Turning this on lets an attacker on the network replace scripts and styles inside an otherwise
        /// secure page, so it is worth doing only for a known host which cannot yet serve everything over
        /// HTTPS. This has no effect on iOS, where the web view offers no equivalent setting.
        /// </remarks>
        public static readonly BindableProperty AllowMixedContentProperty = BindableProperty.Create(
            nameof(AllowMixedContent),
            typeof(bool),
            typeof(CustomWebView),
            false);

        /// <inheritdoc cref="AllowMixedContentProperty" />
        public bool AllowMixedContent
        {
            get => (bool)this.GetValue(AllowMixedContentProperty);
            set => this.SetValue(AllowMixedContentProperty, value);
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

        /// <summary>
        /// Detaches the forwarding of <see cref="WebView.Navigating" /> and <see cref="WebView.Navigated" />
        /// to <see cref="NavigatingCommand" /> and <see cref="NavigatedCommand" />.
        /// </summary>
        /// <remarks>
        /// The subscriptions are to this instance's own events, so they keep nothing else alive and
        /// disposing is not required. Once disposed, the commands are no longer raised and the control
        /// cannot be reused.
        /// </remarks>
        public void Dispose()
        {
            this.Navigating -= this.OnNavigating;
            this.Navigated -= this.OnNavigated;
        }
    }
}