using Microsoft.Maui.Platform;
using UIKit;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.iOS.Utils;

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

        protected override MauiTextField CreatePlatformView()
        {
            var mauiDatePicker = base.CreatePlatformView();
            this.SetupUIToolbar(mauiDatePicker);
            return mauiDatePicker;
        }

        private void SetupUIToolbar(MauiTextField mauiTextField)
        {
            if (mauiTextField.InputAccessoryView == null)
            {
                var toolbar = UIToolbarHelper.CreateUIToolbar(new[] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) });
                mauiTextField.InputAccessoryView = toolbar;
            }

            this.InsertOrUpdateDoneButton(mauiTextField);
        }

        private static void UpdateDoneButtonText(EntryHandler entryHandler, Entry entry)
        {
            entryHandler.InsertOrUpdateDoneButton(entryHandler.PlatformView);
        }

        protected virtual void InsertOrUpdateDoneButton(MauiTextField mauiTextField)
        {
            var newDoneButton = UIToolbarHelper.CreateDoneButton((BindableObject)this.VirtualView, this.HandleDoneButton);
            UIToolbarHelper.ReplaceDoneButton(mauiTextField.InputAccessoryView, newDoneButton);
        }

        private void HandleDoneButton(object sender, EventArgs args)
        {
            this.PlatformView.ResignFirstResponder();
            ((IEntryController)this.VirtualView).SendCompleted();
        }
    }
}