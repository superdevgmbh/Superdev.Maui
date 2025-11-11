using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Utils;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<Picker, PickerHandler>;

    public class PickerHandler : Microsoft.Maui.Handlers.PickerHandler
    {
        private MauiDoneAccessoryView inputAccessoryView;

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

            this.inputAccessoryView = new MauiDoneAccessoryView();
            this.inputAccessoryView.SetDoneButtonAction(this.OnDoneClicked);
            mauiPicker.InputAccessoryView = this.inputAccessoryView;

            return mauiPicker;
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
        // }

        protected override void DisconnectHandler(MauiPicker platformView)
        {
            platformView.InputAccessoryView = null;
            this.inputAccessoryView?.Dispose();
            this.inputAccessoryView = null;

            // ThemeHelper.Current.ThemeChanged -= this.OnThemeChanged;
            base.DisconnectHandler(platformView);
        }

        private void OnDoneClicked()
        {
            ReflectionHelper.RunMethod(this, "OnDone");
        }

        private static void MapDoneButtonText(PickerHandler pickerHandler, Picker picker)
        {
            pickerHandler.DoneButtonText(picker);
        }

        private void DoneButtonText(Picker picker)
        {
            var doneButtonText = DialogExtensions.GetDoneButtonText(picker);
            var mauiPicker = this.PlatformView;
            mauiPicker.InputAccessoryView = MauiDoneAccessoryView.SetDoneButtonText(ref this.inputAccessoryView, doneButtonText);
        }
    }
}