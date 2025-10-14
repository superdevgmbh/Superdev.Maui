using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using MauiDoneAccessoryView = Superdev.Maui.Platforms.Controls.MauiDoneAccessoryView;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<Entry, EntryHandler>;

    public class EntryHandler : Microsoft.Maui.Handlers.EntryHandler
    {
        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.EntryHandler.Mapper)
        {
            [nameof(DialogExtensions.DoneButtonText)] = UpdateDoneButtonText,
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

            var inputAccessoryView = new MauiDoneAccessoryView();
            inputAccessoryView.SetDataContext(this);
            inputAccessoryView.SetDoneClicked(this.OnDoneClicked);

            mauiTextField.InputAccessoryView = inputAccessoryView;

            return mauiTextField;
        }

        private void OnDoneClicked(object _)
        {
            this.PlatformView.ResignFirstResponder();
            this.VirtualView.SendCompleted();
        }

        private static void UpdateDoneButtonText(EntryHandler entryHandler, Entry entry)
        {
            if (entryHandler.PlatformView.InputAccessoryView is MauiDoneAccessoryView mauiDoneAccessoryView)
            {
                var doneButtonText = DialogExtensions.GetDoneButtonText(entry);
                if (doneButtonText != null)
                {
                    mauiDoneAccessoryView.SetDoneText(doneButtonText);
                }
            }
        }
    }
}