using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Utils;
using MauiDoneAccessoryView = Superdev.Maui.Platforms.Controls.MauiDoneAccessoryView;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<Picker, PickerHandler>;

    public class PickerHandler : Microsoft.Maui.Handlers.PickerHandler
    {
        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.PickerHandler.Mapper)
        {
            [DialogExtensions.DoneButtonText] = MapDoneButtonText,
        };

        public PickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public PickerHandler()
            : base(Mapper)
        {
        }

        private new Picker VirtualView => (Picker)base.VirtualView;

        protected override MauiPicker CreatePlatformView()
        {
            var mauiPicker = base.CreatePlatformView();

            var inputAccessoryView = new MauiDoneAccessoryView();
            inputAccessoryView.SetDataContext(this);
            inputAccessoryView.SetDoneClicked(this.OnDoneClicked);

            mauiPicker.InputAccessoryView = inputAccessoryView;

            return mauiPicker;
        }

        private void OnDoneClicked(object obj)
        {
            ReflectionHelper.RunMethod(this, "OnDone");
        }

        private static void MapDoneButtonText(PickerHandler pickerHandler, Picker picker)
        {
            if (pickerHandler.PlatformView.InputAccessoryView is MauiDoneAccessoryView mauiDoneAccessoryView)
            {
                var doneButtonText = DialogExtensions.GetDoneButtonText(picker);
                if (doneButtonText != null)
                {
                    mauiDoneAccessoryView.SetDoneText(doneButtonText);
                }
            }
        }

        protected override void ConnectHandler(MauiPicker platformView)
        {
#if !NET9_0_OR_GREATER
            this.VirtualView.AddCleanUpEvent();
#endif
            base.ConnectHandler(platformView);
            // ThemeHelper.Current.ThemeChanged += this.OnThemeChanged;
        }

        // TODO: React on theme change to update tint color of InputAccessoryView
        // private void OnThemeChanged(object sender, AppTheme e)
        // {
        //     if (this.PlatformView.InputAccessoryView is MauiDoneAccessoryView mauiDoneAccessoryView)
        //     {
        //         var tintColor = UIBarButtonItem.AppearanceWhenContainedIn(typeof(UIToolbar)).TintColor;
        //         mauiDoneAccessoryView.TintColor = tintColor;
        //         mauiDoneAccessoryView.SetNeedsDisplay();
        //     }
        // }

        protected override void DisconnectHandler(MauiPicker platformView)
        {
            // ThemeHelper.Current.ThemeChanged -= this.OnThemeChanged;
            base.DisconnectHandler(platformView);
        }
    }
}