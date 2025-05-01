using CoreGraphics;
using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;
using Superdev.Maui.Services;
using Superdev.Maui.Utils;
using UIKit;

namespace Superdev.Maui.Platforms.Services
{
    public class ActivityIndicatorService : IActivityIndicatorService, IDisposable
    {
        private static readonly Lazy<IActivityIndicatorService> Implementation = new Lazy<IActivityIndicatorService>(CreateActivityIndicatorService, LazyThreadSafetyMode.PublicationOnly);

        public static IActivityIndicatorService Current => Implementation.Value;

        private static IActivityIndicatorService CreateActivityIndicatorService()
        {
            return new ActivityIndicatorService();
        }

        private ActivityIndicatorService()
        {
        }

        private UIView nativeView;
        private ContentPage activityIndicatorPage;

        public void Init<T>(T activityIndicatorPage) where T : ContentPage, IActivityIndicatorPage
        {
            if (this.activityIndicatorPage != null)
            {
                throw new InvalidOperationException($"{nameof(this.Init)} can only be called once.");
            }

            this.activityIndicatorPage = activityIndicatorPage ?? throw new ArgumentException(nameof(activityIndicatorPage));
        }

        private void RenderPage()
        {
            var mainPage = Application.Current?.MainPage;
            if (mainPage == null)
            {
                return;
            }

            var contentPage = this.activityIndicatorPage;
            contentPage.Layout(new Rect(0, 0, mainPage.Width, mainPage.Height));

            if (contentPage.Handler == null)
            {
                var pageHandler = PageHelper.CreatePageHandler(mainPage, contentPage);
                contentPage.Handler = pageHandler;
            }

            this.nativeView = contentPage.Handler.PlatformView as UIView;
        }

        public void ShowLoadingPage(string text)
        {
            this.activityIndicatorPage ??= new DefaultActivityIndicatorPage();

            if (this.nativeView == null)
            {
                this.RenderPage();
            }

            if (this.activityIndicatorPage is IActivityIndicatorPage activityIndicatorPage)
            {
                activityIndicatorPage.SetCaption(text);
            }

            if (this.nativeView != null)
            {
                var rootViewController = WindowStateManager.Default.GetCurrentUIViewController()
                                         ?? throw new InvalidOperationException($"{nameof(WindowStateManager.Default.GetCurrentUIViewController)} returned null.");

                var rootView = rootViewController.View;

                // Set the frame to match the rootView size
                this.nativeView.Frame = rootView.Bounds;

                // Important: disable autoresizing mask so AutoLayout works if needed
                this.nativeView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

                // Add the native view
                rootView.AddSubview(this.nativeView);
                rootView.BringSubviewToFront(this.nativeView);
            }
        }

        public void HideLoadingPage()
        {
            if (this.nativeView != null)
            {
                this.nativeView.RemoveFromSuperview();
            }
        }

        public void Dispose()
        {
            this.activityIndicatorPage?.Handler?.DisconnectHandler();
            this.activityIndicatorPage = null;

            this.nativeView?.Dispose();
            this.nativeView = null;
        }
    }
}