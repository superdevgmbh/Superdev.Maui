using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.Controls;
using Superdev.Maui.Utils;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<Picker, PickerHandler>;

    public class PickerHandler : Microsoft.Maui.Handlers.PickerHandler
    {
        /// <summary>
        /// MAUI commits the selected row to the virtual view in a private method when its own done button is
        /// tapped. This handler replaces that button, so it calls the method by name; the lookup is cached.
        /// </summary>
        private static readonly Lazy<MethodInfo?> OnDoneMethod = new Lazy<MethodInfo?>(
            () => ReflectionHelper.GetMethodInfo(typeof(Microsoft.Maui.Handlers.PickerHandler), "OnDone"));

        private MauiDoneAccessoryView? inputAccessoryView;
        private InputViewHeightKeeper? inputViewHeightKeeper;

        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.PickerHandler.Mapper)
        {
            [DialogExtensions.DoneButtonText] = MapDoneButtonText,
        };

        public PickerHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public PickerHandler()
            : base(Mapper)
        {
        }

        public new Picker? VirtualView => ((ElementHandler)this).VirtualView as Picker;

        public new MauiPicker? PlatformView => ((ElementHandler)this).PlatformView as MauiPicker;

        protected override MauiPicker CreatePlatformView()
        {
            var mauiPicker = base.CreatePlatformView();

            this.inputAccessoryView = new MauiDoneAccessoryView();
            this.inputAccessoryView.SetDoneButtonAction(this.OnDoneClicked);
            mauiPicker.InputAccessoryView = this.inputAccessoryView;
            this.inputViewHeightKeeper = new InputViewHeightKeeper(mauiPicker);

            return mauiPicker;
        }

        // TODO: Update the tint color of the input accessory view when the theme changes.
        protected override void DisconnectHandler(MauiPicker platformView)
        {
            platformView.InputAccessoryView = null;
            this.inputAccessoryView?.Dispose();
            this.inputAccessoryView = null;
            this.inputViewHeightKeeper?.Dispose();
            this.inputViewHeightKeeper = null;

            base.DisconnectHandler(platformView);
        }

        private void OnDoneClicked()
        {
            var onDone = OnDoneMethod.Value;
            if (onDone != null)
            {
                onDone.Invoke(this, null);
                return;
            }

            // A MAUI version without the method: the selection is not committed here, but the picker still closes.
            IPlatformApplication.Current?.Services.GetService<ILogger<PickerHandler>>()?.LogWarning(
                "Microsoft.Maui.Handlers.PickerHandler.OnDone was not found; the selected item may not be committed");
            this.PlatformView?.ResignFirstResponder();
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