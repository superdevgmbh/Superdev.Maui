using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<Editor, EditorHandler>;

    public class EditorHandler : Microsoft.Maui.Handlers.EditorHandler
    {
        private MauiDoneAccessoryView inputAccessoryView;

        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.EditorHandler.Mapper)
        {
            [nameof(DialogExtensions.DoneButtonText)] = MapDoneButtonText,
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

            this.inputAccessoryView = new MauiDoneAccessoryView();
            this.inputAccessoryView.SetDoneButtonAction(this.OnDoneClicked);
            mauiTextView.InputAccessoryView = this.inputAccessoryView;

            return mauiTextView;
        }

        protected override void DisconnectHandler(MauiTextView platformView)
        {
            platformView.InputAccessoryView = null;
            this.inputAccessoryView?.Dispose();
            this.inputAccessoryView = null;

            base.DisconnectHandler(platformView);
        }

        private void OnDoneClicked()
        {
            this.PlatformView.ResignFirstResponder();
            this.VirtualView.SendCompleted();
        }

        private static void MapDoneButtonText(EditorHandler editorHandler, Editor editor)
        {
            editorHandler.DoneButtonText(editor);
        }

        private void DoneButtonText(Editor editor)
        {
            var doneButtonText = DialogExtensions.GetDoneButtonText(editor);
            var mauiTextView = this.PlatformView;
            mauiTextView.InputAccessoryView = MauiDoneAccessoryView.SetDoneButtonText(ref this.inputAccessoryView, doneButtonText);
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
            textView.TextContainer.LineFragmentPadding = 0;
            textView.TextContainerInset = new UIEdgeInsets(5.5f, 2.5f, 5.5f, 2.5f);
        }
    }
}