﻿namespace Superdev.Maui.Controls
{
    public class DrilldownButtonView : DrilldownView, IDrilldownButtonView
    {
        public static readonly BindableProperty ImageSourceProperty =
            BindableProperty.Create(
                nameof(ImageSource),
                typeof(ImageSource),
                typeof(DrilldownButtonView));

        public ImageSource ImageSource
        {
            get => (ImageSource)this.GetValue(ImageSourceProperty);
            set => this.SetValue(ImageSourceProperty, value);
        }

        public static readonly BindableProperty IsBusyProperty =
            BindableProperty.Create(
                nameof(IsBusy),
                typeof(bool),
                typeof(DrilldownButtonView),
                false,
                BindingMode.OneWay);

        public bool IsBusy
        {
            get => (bool)this.GetValue(IsBusyProperty);
            set => this.SetValue(IsBusyProperty, value);
        }
    }
}