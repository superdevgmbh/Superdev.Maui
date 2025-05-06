namespace Superdev.Maui.Controls
{
    public static class DialogExtensions
    {
        public const string DoneButtonText = "DoneButtonText";
        public const string NeutralButtonText = "NeutralButtonText";

        public static readonly BindableProperty DoneButtonTextProperty =
            BindableProperty.CreateAttached(
                DoneButtonText,
                typeof(string),
                typeof(DialogExtensions),
                null);

        public static string GetDoneButtonText(BindableObject view)
        {
            return (string)view.GetValue(DoneButtonTextProperty);
        }

        public static void SetDoneButtonText(BindableObject view, string value)
        {
            view.SetValue(DoneButtonTextProperty, value);
        }

        public static readonly BindableProperty NegativeButtonTextProperty =
            BindableProperty.CreateAttached(
                "NegativeButtonText",
                typeof(string),
                typeof(DialogExtensions),
                null);

        public static string GetNegativeButtonText(BindableObject view)
        {
            return (string)view.GetValue(NegativeButtonTextProperty);
        }

        public static void SetNegativeButtonText(BindableObject view, string value)
        {
            view.SetValue(NegativeButtonTextProperty, value);
        }

        public static readonly BindableProperty PositiveButtonTextProperty =
            BindableProperty.CreateAttached(
                "PositiveButtonText",
                typeof(string),
                typeof(DialogExtensions),
                null);

        public static string GetPositiveButtonText(BindableObject view)
        {
            return (string)view.GetValue(PositiveButtonTextProperty);
        }

        public static void SetPositiveButtonText(BindableObject view, string value)
        {
            view.SetValue(PositiveButtonTextProperty, value);
        }

        public static readonly BindableProperty NeutralButtonTextProperty =
            BindableProperty.CreateAttached(
                NeutralButtonText,
                typeof(string),
                typeof(DialogExtensions),
                null);

        public static string GetNeutralButtonText(BindableObject view)
        {
            return (string)view.GetValue(NeutralButtonTextProperty);
        }

        public static void SetNeutralButtonText(BindableObject view, string value)
        {
            view.SetValue(NeutralButtonTextProperty, value);
        }
    }
}