using System.Diagnostics;

namespace Superdev.Maui.Controls
{
    public static class DebugHelper
    {
        public static bool ShowLayoutBounds =
#if DEBUG
            false;
#else
            false;
#endif

        [Conditional("DEBUG")]
        internal static void DebugLayoutBounds(this ValidatableEntry validatableEntry)
        {
            if (!ShowLayoutBounds)
            {
                return;
            }

            validatableEntry.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentBright");
            validatableEntry.AnnotationLabel.SetValue(VisualElement.BackgroundColorProperty, Colors.Yellow);
            validatableEntry.Entry.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
            validatableEntry.ReadonlyLabel.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
        }

        // [Conditional("DEBUG")]
        // internal static void DebugLayoutBounds(this ValidatableAutoCompleteView element, bool debug = true)
        // {
        //     if (!ShowLayoutBounds || !debug)
        //     {
        //         return;
        //     }
        //
        //     element.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentBright");
        //     element.AnnotationLabel.SetValue(VisualElement.BackgroundColorProperty, Colors.Yellow);
        //     element.Entry.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
        //     element.ReadonlyLabel.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
        // }


        [Conditional("DEBUG")]
        internal static void DebugLayoutBounds(this ValidatableDatePicker element)
        {
            if (!ShowLayoutBounds)
            {
                return;
            }

            element.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentBright");
            element.AnnotationLabel.SetValue(VisualElement.BackgroundColorProperty, Colors.Yellow);
            element.DatePicker.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
            element.ReadonlyLabel.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
        }

        [Conditional("DEBUG")]
        internal static void DebugLayoutBounds(this ValidatableDateTimePicker element)
        {
            if (!ShowLayoutBounds)
            {
                return;
            }

            element.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentBright");
            element.AnnotationLabel.SetValue(VisualElement.BackgroundColorProperty, Colors.Yellow);
            element.DatePicker.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
            element.TimePicker.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
            element.ReadonlyLabel.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
        }

        [Conditional("DEBUG")]
        internal static void DebugLayoutBounds(this ValidatableEditor validatableEditor)
        {
            if (!ShowLayoutBounds)
            {
                return;
            }

            validatableEditor.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentBright");
            validatableEditor.AnnotationLabel.SetValue(VisualElement.BackgroundColorProperty, Colors.Yellow);
            validatableEditor.Editor.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
        }
    }
}