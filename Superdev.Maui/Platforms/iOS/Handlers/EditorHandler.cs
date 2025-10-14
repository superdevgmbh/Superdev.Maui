using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using UIKit;

using MauiDoneAccessoryView = Superdev.Maui.Platforms.Controls.MauiDoneAccessoryView;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<Editor, EditorHandler>;

    public class EditorHandler : Microsoft.Maui.Handlers.EditorHandler
    {
        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.EditorHandler.Mapper)
        {
            [nameof(DialogExtensions.DoneButtonText)] = UpdateDoneButtonText,
        };

        public EditorHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public EditorHandler()
            : base(Mapper)
        {
        }

        private new Editor VirtualView => (Editor)base.VirtualView;

        protected override MauiTextView CreatePlatformView()
        {
            var mauiTextView = base.CreatePlatformView();

            var inputAccessoryView = new MauiDoneAccessoryView();
            inputAccessoryView.SetDataContext(this);
            inputAccessoryView.SetDoneClicked(this.OnDoneClicked);

            mauiTextView.InputAccessoryView = inputAccessoryView;

            return mauiTextView;
        }

        private void OnDoneClicked(object _)
        {
            this.PlatformView.ResignFirstResponder();
            this.VirtualView.SendCompleted();
        }

        private static void UpdateDoneButtonText(EditorHandler editorHandler, Editor editor)
        {
            if (editorHandler.PlatformView.InputAccessoryView is MauiDoneAccessoryView mauiDoneAccessoryView)
            {
                var doneButtonText = DialogExtensions.GetDoneButtonText(editor);
                if (doneButtonText != null)
                {
                    mauiDoneAccessoryView.SetDoneText(doneButtonText);
                }
            }
        }

        protected override void ConnectHandler(MauiTextView platformView)
        {
            base.ConnectHandler(platformView);
            this.UpdateTextInsets();
            this.FixScrollingIssue();
        }

        private void FixScrollingIssue()
        {
            var editor = this.VirtualView;
            if (editor.AutoSize == EditorAutoSizeOption.TextChanges)
            {
                this.PlatformView.ScrollEnabled = false;
            }
        }

        private void UpdateTextInsets()
        {
            var textView = this.PlatformView;
            // textView.TextContainerInset = new UIEdgeInsets(new nfloat(5.5), new nfloat(2.5), new nfloat(5.5), new nfloat(2.5));
            textView.TextContainer.LineFragmentPadding = 0;
            textView.TextContainerInset = new UIEdgeInsets(5.5f, 2.5f, 5.5f, 2.5f);
        }
    }
}