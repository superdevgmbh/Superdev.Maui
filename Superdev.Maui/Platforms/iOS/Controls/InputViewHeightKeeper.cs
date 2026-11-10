using System.Runtime.InteropServices;
using Foundation;
using UIKit;

namespace Superdev.Maui.Platforms.Controls
{
    /// <summary>
    /// Keeps a picker's Done bar on its wheel after the interface rotated while the picker was open.
    /// </summary>
    /// <remarks>
    /// After a rotation back to portrait, UIKit sizes the keyboard area like the system keyboard, which is taller than
    /// the wheel and the Done bar together. The bar sits at the top of that area and the wheel at the bottom, with a gap
    /// between them. Reloading the input views makes UIKit size the area from the views again.
    /// </remarks>
    internal sealed class InputViewHeightKeeper : IDisposable
    {
        /// <summary>
        /// How much taller than the wheel and the bar the keyboard area may be before it is sized again. UIKit lets the
        /// two overlap by a few points in some orientations.
        /// </summary>
        private const int Tolerance = 8;

        private readonly WeakReference<UITextField> textField;
        private NSObject? keyboardFrameObserver;
        private NFloat keyboardWidth;
        private bool reloaded;

        public InputViewHeightKeeper(UITextField textField)
        {
            this.textField = new WeakReference<UITextField>(textField);
            this.keyboardFrameObserver = UIKeyboard.Notifications.ObserveDidChangeFrame(this.OnKeyboardDidChangeFrame);
        }

        private void OnKeyboardDidChangeFrame(object? sender, UIKeyboardEventArgs e)
        {
            if (!this.textField.TryGetTarget(out var textField) || !textField.IsFirstResponder || textField.InputView is not UIView inputView)
            {
                return;
            }

            // The reload changes the keyboard frame again. Reloading once per orientation keeps a keyboard area that
            // stays too tall from reloading over and over; a new keyboard width means a new orientation.
            var keyboardFrame = e.FrameEnd;
            if (keyboardFrame.Width != this.keyboardWidth)
            {
                this.keyboardWidth = keyboardFrame.Width;
                this.reloaded = false;
            }

            // During the rotation the keyboard frame still has the old width; only the settled frame is compared.
            if (this.reloaded || keyboardFrame.Width != inputView.Frame.Width)
            {
                return;
            }

            var inputViewsHeight = inputView.Frame.Height + (textField.InputAccessoryView?.Frame.Height ?? 0);
            if (keyboardFrame.Height - inputViewsHeight <= Tolerance)
            {
                return;
            }

            this.reloaded = true;
            textField.ReloadInputViews();
        }

        public void Dispose()
        {
            this.keyboardFrameObserver?.Dispose();
            this.keyboardFrameObserver = null;
        }
    }
}
