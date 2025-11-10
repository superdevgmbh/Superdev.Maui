using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<Entry, EntryHandler>;

    public class EntryHandler : Microsoft.Maui.Handlers.EntryHandler
    {
        private MauiDoneAccessoryView inputAccessoryView;

        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.EntryHandler.Mapper)
        {
            [nameof(DialogExtensions.DoneButtonText)] = MapDoneButtonText,
        };

        public EntryHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public EntryHandler()
            : base(Mapper)
        {
        }

        private new Entry VirtualView => (Entry)base.VirtualView;

        protected override MauiTextField CreatePlatformView()
        {
            var mauiTextField = base.CreatePlatformView();

            this.inputAccessoryView = new MauiDoneAccessoryView();
            this.inputAccessoryView.SetDoneButtonAction(this.OnDoneClicked);
            mauiTextField.InputAccessoryView = this.inputAccessoryView;

            return mauiTextField;
        }

        protected override void DisconnectHandler(MauiTextField platformView)
        {
            platformView.InputAccessoryView = null;
            this.inputAccessoryView?.Dispose();
            this.inputAccessoryView = null;

            base.DisconnectHandler(platformView);
        }

        private static void MapDoneButtonText(EntryHandler entryHandler, Entry entry)
        {
            entryHandler.DoneButtonText(entry);
        }

        private void DoneButtonText(Entry entry)
        {
            var doneButtonText = DialogExtensions.GetDoneButtonText(entry);
            var mauiTextField = this.PlatformView;
            mauiTextField.InputAccessoryView = MauiDoneAccessoryView.SetDoneButtonText(ref this.inputAccessoryView, doneButtonText);
        }

        private void OnDoneClicked()
        {
            this.PlatformView.ResignFirstResponder();
            this.VirtualView.SendCompleted();
        }
    }
}