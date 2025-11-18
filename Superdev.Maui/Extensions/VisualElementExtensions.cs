using Superdev.Maui.Controls;

namespace Superdev.Maui.Extensions
{
    public static class VisualElementExtensions
    {
        public static Task<bool> AnimateAsync(this VisualElement element, string name, Animation animation, uint steps, uint length, Easing easing)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            element.Animate(name, animation, steps, length, easing, (v, c) => taskCompletionSource.SetResult(c));
            return taskCompletionSource.Task;
        }

        internal static Entry AsEntry(this VisualElement bindable)
        {
            if (bindable is Entry entry)
            {
                return entry;
            }

            if (bindable is ValidatableEntry validatableEntry)
            {
                return validatableEntry.Entry;
            }

            return null;
        }

        internal static Editor AsEditor(this VisualElement bindable)
        {
            if (bindable is Editor editor)
            {
                return editor;
            }

            if (bindable is ValidatableEditor validatableEditor)
            {
                return validatableEditor.Editor;
            }

            return null;
        }

        internal static VisualElement AsVisualElement(this VisualElement bindable)
        {
            if (bindable is VisualElement visualElement)
            {
                return visualElement;
            }

            if (bindable is ValidatableEntry validatableEntry)
            {
                return validatableEntry.Entry;
            }

            return null;
        }
    }
}