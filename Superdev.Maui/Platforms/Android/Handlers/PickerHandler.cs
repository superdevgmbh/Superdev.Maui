#nullable enable
using Android.Text;
using Android.Text.Style;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.Handlers.MauiFix.Extensions;
using Superdev.Maui.Utils;
using AppCompatAlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace Superdev.Maui.Platforms.Handlers
{
    public class PickerHandler : Microsoft.Maui.Handlers.PickerHandler
    {
        private static readonly TimeSpan TimeToIgnoreClickAfterFocus = TimeSpan.FromMilliseconds(100F);

        private AppCompatAlertDialog? dialog;

        public PickerHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
            : base(mapper, commandMapper)
        {
        }

        public PickerHandler()
        {
        }

        private new Picker VirtualView => (Picker)base.VirtualView;

        protected override MauiPicker CreatePlatformView()
        {
            var mauiPicker = new MauiPicker(this.Context);
            return mauiPicker;
        }

        protected override void ConnectHandler(MauiPicker mauiPicker)
        {
            var visualElement = (VisualElement)this.VirtualView;
            visualElement.Loaded += this.OnVisualElementLoaded;
            visualElement.Unloaded += this.OnVisualElementUnloaded;

#if !NET9_0_OR_GREATER
            this.VirtualView.AddCleanUpEvent();
#endif

            // base.ConnectHandler(platformView);
        }

        private async void OnVisualElementLoaded(object? sender, EventArgs e)
        {
            await Task.Delay(TimeToIgnoreClickAfterFocus);

            var platformView = this.PlatformView;
            platformView.FocusChange += this.OnFocusChange;
            platformView.Click += this.OnClick;
        }

        private void OnVisualElementUnloaded(object? sender, EventArgs e)
        {
            if (((ViewHandler)this).PlatformView is MauiPicker mauiPicker)
            {
                mauiPicker.FocusChange -= this.OnFocusChange;
                mauiPicker.Click -= this.OnClick;
            }
        }

        protected override void DisconnectHandler(MauiPicker mauiPicker)
        {
            var visualElement = (VisualElement)this.VirtualView;
            visualElement.Unloaded -= this.OnVisualElementUnloaded;
            visualElement.Loaded -= this.OnVisualElementLoaded;

            mauiPicker.FocusChange -= this.OnFocusChange;
            mauiPicker.Click -= this.OnClick;

            // base.DisconnectHandler(platformView);
        }

        private void OnFocusChange(object? sender, global::Android.Views.View.FocusChangeEventArgs e)
        {
            if (this.PlatformView == null)
            {
                return;
            }

            if (e.HasFocus)
            {
                if (this.PlatformView.Clickable)
                {
                    this.PlatformView.CallOnClick();
                }
                else
                {
                    this.OnClick(this.PlatformView, EventArgs.Empty);
                }
            }
            else if (this.dialog != null)
            {
                this.dialog.Hide();
                this.dialog = null;
            }
        }

        private void OnClick(object? sender, EventArgs e)
        {
            if (this.dialog == null && this.VirtualView is Picker picker)
            {
                using (var builder = new AppCompatAlertDialog.Builder(this.Context))
                {
                    if (!string.IsNullOrWhiteSpace(picker.Title))
                    {
                        if (picker.TitleColor == null)
                        {
                            builder.SetTitle(picker.Title ?? string.Empty);
                        }
                        else
                        {
                            var title = new SpannableString(picker.Title ?? string.Empty);
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-android/issues/6962
                            title.SetSpan(new ForegroundColorSpan(picker.TitleColor.ToPlatform()), 0, title.Length(),
                                SpanTypes.ExclusiveExclusive);
#pragma warning restore CA1416
                            builder.SetTitle(title);
                        }
                    }

                    var items = picker.GetItemsAsArray();

                    for (var i = 0; i < items.Length; i++)
                    {
                        var item = items[i];
                        if (item == null)
                        {
                            items[i] = string.Empty;
                        }
                    }

                    builder.SetItems(items, (_, e) =>
                    {
                        var selectedIndex = e.Which;
                        picker.SelectedIndex = selectedIndex;
                        this.PlatformView?.UpdatePicker(picker);
                    });

                    var negativeButtonText = GetNegativeButtonText(picker);
                    builder.SetNegativeButton(negativeButtonText, (_, _) => { });

                    this.dialog = builder.Create();
                }

                if (this.dialog == null)
                {
                    return;
                }

                this.dialog.UpdateFlowDirection(this.PlatformView);

                this.dialog.SetCanceledOnTouchOutside(true);

                EventHandler dismissEvent = null;
                dismissEvent = (_, _) =>
                {
                    this.dialog.DismissEvent -= dismissEvent;
                    this.dialog = null;
                };
                this.dialog.DismissEvent += dismissEvent;

                this.dialog.Show();
            }
        }

        private static string GetNegativeButtonText(BindableObject element)
        {
            if (DialogExtensions.GetNegativeButtonText(element) is string negativeButtonText)
            {
                return negativeButtonText;
            }

            negativeButtonText = AApplication.Context.Resources.GetString(AR.String.Cancel);
            return negativeButtonText;
        }
    }
}