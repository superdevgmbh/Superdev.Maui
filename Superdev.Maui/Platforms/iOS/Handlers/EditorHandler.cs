using Microsoft.Maui.Platform;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    public class EditorHandler : Microsoft.Maui.Handlers.EditorHandler
    {
        public EditorHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public EditorHandler()
            : base(Mapper)
        {
        }

        protected override void ConnectHandler(MauiTextView platformView)
        {
            base.ConnectHandler(platformView);
            this.UpdateTextInsets();
            this.FixScrollingIssue();
        }

        private void FixScrollingIssue()
        {
            var editor = (Editor)this.VirtualView;
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