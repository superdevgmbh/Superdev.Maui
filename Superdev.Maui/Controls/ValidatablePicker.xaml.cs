﻿using System.Collections;
using System.Diagnostics;
using Superdev.Maui.Utils;

namespace Superdev.Maui.Controls
{
    public partial class ValidatablePicker : Grid
    {
        private const int SelectedIndexDefaultValue = -1;

        public ValidatablePicker()
        {
            this.InitializeComponent();
            this.DebugLayoutBounds();
        }

        [Conditional("DEBUG")]
        private void DebugLayoutBounds(bool debug = true)
        {
            if (!DebugHelper.ShowLayoutBounds || !debug)
            {
                return;
            }

            this.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentBright");
            this.AnnotationLabel.SetValue(VisualElement.BackgroundColorProperty, Colors.Yellow);
            this.Picker.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
            this.ReadonlyLabel.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
        }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(
                nameof(Placeholder),
                typeof(string),
                typeof(ValidatablePicker),
                null,
                BindingMode.OneWay,
                null,
                OnPlaceholderPropertyChanged);

        private static void OnPlaceholderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatablePicker)bindable;
            picker.OnPropertyChanged(nameof(picker.AnnotationText));
        }

        public string Placeholder
        {
            get => (string)this.GetValue(PlaceholderProperty);
            set => this.SetValue(PlaceholderProperty, value);
        }

        public string AnnotationText
        {
            get
            {
                if (this.SelectedIndex != SelectedIndexDefaultValue || this.IsReadonly)
                {
                    return this.Placeholder;
                }

                return " ";
            }
        }

        public static readonly BindableProperty PickerStyleProperty =
            BindableProperty.Create(
                nameof(PickerStyle),
                typeof(Style),
                typeof(ValidatablePicker),
                default(Style),
                BindingMode.OneWay);

        public Style PickerStyle
        {
            get => (Style)this.GetValue(PickerStyleProperty);
            set => this.SetValue(PickerStyleProperty, value);
        }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(ValidatablePicker),
                null,
                BindingMode.OneWay,
                null,
                OnItemsSourcePropertyChanged);

        private static void OnItemsSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatablePicker)bindable;
            picker.OnPropertyChanged(nameof(picker.Placeholder));
            picker.OnPropertyChanged(nameof(picker.AnnotationText));
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(
                nameof(SelectedItem),
                typeof(object),
                typeof(ValidatablePicker),
                null,
                BindingMode.TwoWay,
                null,
                OnSelectedItemPropertyChanged);

        private static void OnSelectedItemPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatablePicker)bindable;
            picker.OnPropertyChanged(nameof(picker.Placeholder));
            picker.OnPropertyChanged(nameof(picker.ReadonlyText));
        }

        public object SelectedItem
        {
            get => this.GetValue(SelectedItemProperty);
            set
            {
                this.SetValue(SelectedItemProperty, value);
                this.OnPropertyChanged(nameof(this.AnnotationText));
            }
        }

        public static readonly BindableProperty SelectedIndexProperty =
            BindableProperty.Create(
                nameof(SelectedIndex),
                typeof(int),
                typeof(ValidatablePicker),
                SelectedIndexDefaultValue,
                BindingMode.TwoWay,
                null,
                OnSelectedIndexPropertyChanged);

        public int SelectedIndex
        {
            get => (int)this.GetValue(SelectedIndexProperty);
            set => this.SetValue(SelectedIndexProperty, value);
        }

        private static void OnSelectedIndexPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatablePicker)bindable;
            picker.OnPropertyChanged(nameof(picker.AnnotationText));
        }

        public static readonly BindableProperty ItemDisplayBindingProperty =
            BindableProperty.Create(
                nameof(ItemDisplayBinding),
                typeof(string),
                typeof(ValidatablePicker),
                null,
                BindingMode.OneWay);

        public string ItemDisplayBinding
        {
            get => (string)this.GetValue(ItemDisplayBindingProperty);
            set => this.SetValue(ItemDisplayBindingProperty, value);
        }

        public static readonly BindableProperty IsReadonlyProperty =
            BindableProperty.Create(
                nameof(IsReadonly),
                typeof(bool),
                typeof(ValidatablePicker),
                false,
                BindingMode.OneWay,
                propertyChanged: OnIsReadonlyPropertyChanged);

        private static void OnIsReadonlyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatablePicker)bindable;
            picker.OnPropertyChanged(nameof(picker.AnnotationText));
        }

        public bool IsReadonly
        {
            get => (bool)this.GetValue(IsReadonlyProperty);
            set => this.SetValue(IsReadonlyProperty, value);
        }

        public static readonly BindableProperty ReadonlyTextProperty =
            BindableProperty.Create(
                nameof(ReadonlyText),
                typeof(string),
                typeof(ValidatablePicker),
                null,
                BindingMode.OneWay);

        public string ReadonlyText
        {
            get
            {
                var readonlyText = (string)this.GetValue(ReadonlyTextProperty);
                if (readonlyText == null)
                {
                    // In case readonly text is null, we try to take SelectedItem as ReadonlyText
                    readonlyText = this.SelectedItem?.ToString();
                }
                return readonlyText;
            }
            set => this.SetValue(ReadonlyTextProperty, value);
        }

        public static readonly BindableProperty ValidationErrorsProperty =
            BindableProperty.Create(
                nameof(ValidationErrors),
                typeof(IEnumerable<string>),
                typeof(ValidatablePicker),
                default(IEnumerable<string>),
                BindingMode.OneWay);

        public IEnumerable<string> ValidationErrors
        {
            get => (IEnumerable<string>)this.GetValue(ValidationErrorsProperty);
            set => this.SetValue(ValidationErrorsProperty, value);
        }
    }
}