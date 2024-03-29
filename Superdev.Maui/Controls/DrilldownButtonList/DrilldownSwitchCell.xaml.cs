﻿using System.Diagnostics;
using System.Windows.Input;

namespace Superdev.Maui.Controls
{
    public partial class DrilldownSwitchCell : ViewCell
    {
        public DrilldownSwitchCell()
        {
            try
            {
                this.InitializeComponent();
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
                typeof(DrilldownSwitchCell),
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
                typeof(DrilldownSwitchCell),
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
                typeof(DrilldownSwitchCell),
                null,
                BindingMode.OneWay);

        public object CommandParameter
        {
            get => this.GetValue(CommandParameterProperty);
            set => this.SetValue(CommandParameterProperty, value);
        }

        public static new readonly BindableProperty IsEnabledProperty =
            BindableProperty.Create(
                nameof(IsEnabled),
                typeof(bool),
                typeof(DrilldownSwitchCell),
                true,
                BindingMode.OneWay);

        public new bool IsEnabled
        {
            get => (bool)this.GetValue(IsEnabledProperty);
            set => this.SetValue(IsEnabledProperty, value);
        }

        public static readonly BindableProperty IsToggledProperty =
            BindableProperty.Create(nameof(IsToggled),
                typeof(bool),
                typeof(DrilldownSwitchCell),
                false,
                BindingMode.TwoWay);

        public bool IsToggled
        {
            get => (bool)this.GetValue(IsToggledProperty);
            set => this.SetValue(IsToggledProperty, (object)value);
        }
    }
}