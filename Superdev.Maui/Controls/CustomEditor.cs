﻿namespace Superdev.Maui.Controls
{
    public class CustomEditor : Editor, IDisposable
    {
        public const int MaxLinesDefault = -1;

        public static readonly BindableProperty MaxLinesProperty =
            BindableProperty.Create(
                nameof(MaxLines),
                typeof(int),
                typeof(CustomEditor),
                MaxLinesDefault);

        public int MaxLines
        {
            get => (int)this.GetValue(MaxLinesProperty);
            set => this.SetValue(MaxLinesProperty, value);
        }

        public static readonly BindableProperty HideKeyboardProperty =
            BindableProperty.Create(
                nameof(HideKeyboard),
                typeof(bool),
                typeof(CustomEditor),
                defaultValue: false);

        public bool HideKeyboard
        {
            get => (bool)this.GetValue(HideKeyboardProperty);
            set => this.SetValue(HideKeyboardProperty, value);
        }

        public CustomEditor()
        {
            this.TextChanged += this.OnTextChanged;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            this.InvalidateMeasure();
        }

        public void Dispose()
        {
            this.TextChanged -= this.OnTextChanged;
        }
    }
}
