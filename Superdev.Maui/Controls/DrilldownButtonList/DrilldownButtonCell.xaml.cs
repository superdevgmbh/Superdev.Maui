﻿using System.Diagnostics;
using System.Windows.Input;
using Superdev.Maui.Styles;

namespace Superdev.Maui.Controls
{
    public partial class DrilldownButtonCell : ViewCell
    {
        public DrilldownButtonCell()
        {
            try
            {
                this.InitializeComponent();

                // Hack: OnPlatform lacks of support for DynamicResource bindings!
                if (Device.RuntimePlatform == Device.Android)
                {
                    this.ActivityIndicator.SetDynamicResource(ActivityIndicator.ColorProperty, ThemeConstants.Color.Secondary);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(
                nameof(Text),
                typeof(string),
                typeof(DrilldownButtonCell),
                null,
                BindingMode.OneWay);

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(
                nameof(Command),
                typeof(ICommand),
                typeof(DrilldownButtonCell),
                null,
                BindingMode.OneWay);

        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(
                nameof(CommandParameter),
                typeof(object),
                typeof(DrilldownButtonCell),
                null,
                BindingMode.OneWay);

        public object CommandParameter
        {
            get => this.GetValue(CommandParameterProperty);
            set => this.SetValue(CommandParameterProperty, value);
        }

        public new static readonly BindableProperty IsEnabledProperty =
            BindableProperty.Create(
                nameof(IsEnabled),
                typeof(bool),
                typeof(DrilldownButtonCell),
                true,
                BindingMode.OneWay);

        public new bool IsEnabled
        {
            get => (bool)this.GetValue(IsEnabledProperty);
            set => this.SetValue(IsEnabledProperty, value);
        }

        public static readonly BindableProperty ImageSourceProperty =
            BindableProperty.Create(
                nameof(ImageSource),
                typeof(ImageSource),
                typeof(DrilldownButtonCell),
                null,
                BindingMode.OneWay);

        public ImageSource ImageSource
        {
            get => (ImageSource)this.GetValue(ImageSourceProperty);
            set => this.SetValue(ImageSourceProperty, value);
        }

        public static readonly BindableProperty IsBusyProperty =
            BindableProperty.Create(
                nameof(IsBusy),
                typeof(bool),
                typeof(DrilldownButtonCell),
                false,
                BindingMode.OneWay);

        public bool IsBusy
        {
            get => (bool)this.GetValue(IsBusyProperty);
            set => this.SetValue(IsBusyProperty, value);
        }
    }
}