using CoreGraphics;
using Superdev.Maui.Extensions;
using UIKit;

namespace Superdev.Maui.Platforms.Controls
{
    /// <summary>
    ///     Source: https://github.com/dotnet/maui/blob/main/src/Core/src/Platform/iOS/MauiDoneAccessoryView.cs
    /// </summary>
    public sealed class MauiDoneAccessoryView : UIToolbar
    {
        private Action doneAction;
        private Action clearAction;
        private UIBarButtonItem doneButton;
        private UIBarButtonItem clearButton;
        private static readonly UIBarButtonItem FlexibleSpace = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);

        public MauiDoneAccessoryView()
            : this(done: null)
        {
        }

        public MauiDoneAccessoryView(Action done)
            : base(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 44))
        {
            this.doneAction = done;
            this.BarStyle = UIBarStyle.Default;
            this.Translucent = true;
            this.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

            var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, this.OnDoneButtonClicked);
            this.SetItems([spacer, doneButton], false);
        }

        public void SetDoneButtonAction(Action done)
        {
            this.doneAction = done;
        }

        private void OnDoneButtonClicked(object sender, EventArgs e)
        {
            this.doneAction?.Invoke();
        }

        public void SetClearButtonAction(Action clear)
        {
            this.clearAction = clear;
        }

        private void OnClearButtonClicked(object sender, EventArgs e)
        {
            this.clearAction?.Invoke();
        }

        public void SetDoneButtonText(string text)
        {
            this.doneButton = !string.IsNullOrEmpty(text)
                ? new UIBarButtonItem(text, UIBarButtonItemStyle.Done, this.OnDoneButtonClicked)
                : null;
            this.RefreshItems();
        }

        public void SetClearButtonText(string text)
        {
            this.clearButton = !string.IsNullOrEmpty(text)
                ? new UIBarButtonItem(text, UIBarButtonItemStyle.Plain, this.OnClearButtonClicked)
                : null;
            this.RefreshItems();
        }

        /// <summary>
        /// Builds the list of items in the following order:
        /// [clear button – flexible space – done button]
        /// </summary>
        private void RefreshItems()
        {
            UIBarButtonItem[] newItems;

            if (this.clearButton != null && this.doneButton != null)
            {
                newItems = [this.clearButton, FlexibleSpace, this.doneButton];
            }
            else
            {
                if (this.clearButton != null)
                {
                    newItems = [this.clearButton, FlexibleSpace];
                }
                else
                {
                    if (this.doneButton != null)
                    {
                        newItems = [FlexibleSpace, this.doneButton];
                    }
                    else
                    {
                        newItems = [];
                    }
                }
            }

            this.SetItems(newItems, false);
            this.SetNeedsDisplay();
        }

        public static MauiDoneAccessoryView SetDoneButtonText(ref MauiDoneAccessoryView inputAccessoryView, string text)
        {
            inputAccessoryView.SetDoneButtonText(text);

            return GetNewInputAccessoryView(inputAccessoryView);
        }

        public static MauiDoneAccessoryView SetClearButtonText(MauiDoneAccessoryView inputAccessoryView, string text)
        {
            inputAccessoryView.SetClearButtonText(text);

            return GetNewInputAccessoryView(inputAccessoryView);
        }

        private static MauiDoneAccessoryView GetNewInputAccessoryView(MauiDoneAccessoryView inputAccessoryView)
        {
            if (!inputAccessoryView.Items!.Any())
            {
                return null;
            }

            return inputAccessoryView;
        }
    }
}