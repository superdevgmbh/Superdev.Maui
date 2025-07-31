using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Controls;
using Superdev.Maui.Services;
using Superdev.Maui.Utils;
using Application = Microsoft.Maui.Controls.Application;

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
            this.logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<ActivityIndicatorService>>();
            this.mainThread = IMainThread.Current;
        }

        private readonly ILogger logger;
        private readonly IMainThread mainThread;
        private AView nativeView;
        private Dialog dialog;
        private ContentPage activityIndicatorPage;

        private static DisplayMetrics GetDisplayMetrics(Context context)
        {
            var displayMetrics = context.Resources.DisplayMetrics;
            return displayMetrics;
        }

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

            var context = Platform.CurrentActivity;
            var displayMetrics = GetDisplayMetrics(context);

            var contentPage = this.activityIndicatorPage;
            this.activityIndicatorPage.Parent = mainPage;
            contentPage.Layout(new Rect(0, 0, mainPage.Width, mainPage.Height));

            if (contentPage.Handler == null)
            {
                var pageHandler = PageHelper.CreatePageHandler(mainPage, contentPage);
                contentPage.Handler = pageHandler;
            }

            this.nativeView = contentPage.Handler.PlatformView as AView;

            this.dialog = new Dialog(context);
            this.dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
            this.dialog.SetCancelable(false);
            this.dialog.SetContentView(this.nativeView);

            var window = this.dialog.Window;
            window.SetLayout(displayMetrics.WidthPixels, displayMetrics.HeightPixels);
            window.SetGravity(GravityFlags.CenterHorizontal | GravityFlags.CenterVertical);
            window.ClearFlags(WindowManagerFlags.DimBehind);
            window.SetBackgroundDrawable(new ColorDrawable(AColor.Transparent));
        }

        public async void ShowLoadingPage(string text)
        {
            try
            {
                this.activityIndicatorPage ??= new DefaultActivityIndicatorPage();

                if (this.nativeView == null)
                {
                    this.RenderPage();
                }

                // Update the caption title
                if (this.activityIndicatorPage is IActivityIndicatorPage contentPage)
                {
                    contentPage.SetTitle(text);
                }

                if (this.dialog is { IsShowing: false })
                {
                    await this.mainThread.InvokeOnMainThreadAsync(() =>
                    {
                        this.dialog.Show();
                    });
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "ShowLoadingPage failed with exception");
            }
        }

        public async void HideLoadingPage()
        {
            try
            {
                await this.mainThread.InvokeOnMainThreadAsync(() =>
                {
                    this.dialog?.Dismiss();
                });
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "HideLoadingPage failed with exception");
            }
        }

        public void OnResume()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.activityIndicatorPage is Page p)
            {
                p.Handler?.DisconnectHandler();
                p.Handler = null;
            }

            this.nativeView?.Dispose();
            this.nativeView = null;

            this.dialog?.Dispose();
            this.dialog = null;
        }
    }
}