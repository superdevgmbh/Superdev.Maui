﻿using System.Collections;

namespace Superdev.Maui.Controls
{
    public partial class DrilldownButtonList : Grid
    {
        public DrilldownButtonList()
        {
            this.InitializeComponent();
        }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(DrilldownButtonList),
                null,
                BindingMode.OneWay);

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(DrilldownButtonList),
                null);

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)this.GetValue(ItemTemplateProperty);
            set => this.SetValue(ItemTemplateProperty, value);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var bc = this.BindingContext;
            foreach (var child in this.ItemsSource.OfType<BindableObject>())
            {
                SetInheritedBindingContext(child, bc);
            }
        }
    }
}