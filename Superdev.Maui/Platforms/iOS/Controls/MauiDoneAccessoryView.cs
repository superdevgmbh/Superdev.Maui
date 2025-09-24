using CoreGraphics;
using Superdev.Maui.Extensions;
using UIKit;

namespace Superdev.Maui.Platforms.Controls
{
    /// <summary>
    ///     Source: https://github.com/dotnet/maui/blob/main/src/Core/src/Platform/iOS/MauiDoneAccessoryView.cs#L8
    /// </summary>
    internal sealed class MauiDoneAccessoryView : UIToolbar
    {
        private readonly BarButtonItemProxy proxy;

        public MauiDoneAccessoryView()
            : base(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 44))
        {
            this.proxy = new BarButtonItemProxy();
            this.BarStyle = UIBarStyle.Default;
            this.Translucent = true;
            var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, this.proxy.OnDataClicked);
            this.SetItems([spacer, doneButton], false);
        }

        internal void SetDoneClicked(Action<object> value)
        {
            this.proxy.SetDoneClicked(value);
        }

        internal void SetDoneText(string text)
        {
            var doneButton = this.Items.LastOrDefault(i => i.Style == UIBarButtonItemStyle.Done);
            if (doneButton != null)
            {
                var newDoneButton = new UIBarButtonItem(text, UIBarButtonItemStyle.Done, this.proxy.OnDataClicked);
                var newItems = this.Items.Replace(doneButton, newDoneButton);

                this.SetItems(newItems, false);
                this.SetNeedsDisplay();
            }
            else
            {
                var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
                doneButton = new UIBarButtonItem(text, UIBarButtonItemStyle.Done, this.proxy.OnDataClicked);

                this.SetItems([spacer, doneButton], false);
                this.SetNeedsDisplay();
            }
        }

        internal void SetDataContext(object dataContext)
        {
            this.proxy.SetDataContext(dataContext);
        }

        public MauiDoneAccessoryView(Action doneClicked) : base(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 44))
        {
            this.proxy = new BarButtonItemProxy(doneClicked);
            this.BarStyle = UIBarStyle.Default;
            this.Translucent = true;

            var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, this.proxy.OnClicked);
            this.SetItems([spacer, doneButton], false);
        }

        private class BarButtonItemProxy
        {
            private readonly Action doneClicked;
            private Action<object> doneWithDataClicked;
            private WeakReference<object> data;

            public BarButtonItemProxy() { }

            public BarButtonItemProxy(Action doneClicked)
            {
                this.doneClicked = doneClicked;
            }

            public void SetDoneClicked(Action<object> value)
            {
                this.doneWithDataClicked = value;
            }

            public void SetDataContext(object dataContext)
            {
                this.data = dataContext is null ? null : new(dataContext);
            }

            public void OnDataClicked(object sender, EventArgs e)
            {
                if (this.data is not null && this.data.TryGetTarget(out var data))
                {
                    this.doneWithDataClicked?.Invoke(data);
                }
            }

            public void OnClicked(object sender, EventArgs e)
            {
                this.doneClicked?.Invoke();
            }
        }
    }
}