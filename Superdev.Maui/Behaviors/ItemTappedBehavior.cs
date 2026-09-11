using System.Globalization;
using System.Windows.Input;

namespace Superdev.Maui.Behaviors
{
    /// <summary>
    /// Runs <see cref="Command"/> with the tapped item of a <see cref="SelectableItemsView"/>
    /// (e.g. <see cref="CollectionView"/>) on every tap, then clears the selection so the same item can be
    /// tapped again. This avoids a sticky selection and removes the need for a dedicated <c>SelectedItem</c>
    /// property in the view model.
    /// </summary>
    /// <remarks>
    /// The behavior forces <see cref="SelectableItemsView.SelectionMode"/> to <see cref="SelectionMode.Single"/>;
    /// do not additionally bind or set <c>SelectionMode</c> on the same control. It works with grouped sources
    /// (the tapped item is always the leaf item, never the group).
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// <CollectionView ItemsSource="{Binding Items}" ItemTemplate="{StaticResource ItemTemplate}">
    ///     <CollectionView.Behaviors>
    ///         <s:ItemTappedBehavior Command="{Binding ItemTappedCommand}" />
    ///     </CollectionView.Behaviors>
    /// </CollectionView>
    /// ]]>
    /// </code>
    /// </example>
    public class ItemTappedBehavior : BehaviorBase<SelectableItemsView>
    {
        private bool isClearing;

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(ItemTappedBehavior));

        /// <summary>
        /// Gets or sets the command that is executed when an item is tapped. The tapped item is passed
        /// as the command parameter unless <see cref="CommandParameter"/> is set.
        /// </summary>
        public ICommand? Command
        {
            get => (ICommand?)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
            nameof(CommandParameter),
            typeof(object),
            typeof(ItemTappedBehavior));

        /// <summary>
        /// Gets or sets an optional fixed command parameter. If set, it is passed to <see cref="Command"/>
        /// instead of the tapped item.
        /// </summary>
        public object? CommandParameter
        {
            get => this.GetValue(CommandParameterProperty);
            set => this.SetValue(CommandParameterProperty, value);
        }

        public static readonly BindableProperty ConverterProperty = BindableProperty.Create(
            nameof(Converter),
            typeof(IValueConverter),
            typeof(ItemTappedBehavior));

        /// <summary>
        /// Gets or sets an optional converter applied to the tapped item (or <see cref="CommandParameter"/>)
        /// before it is passed to <see cref="Command"/>.
        /// </summary>
        public IValueConverter? Converter
        {
            get => (IValueConverter?)this.GetValue(ConverterProperty);
            set => this.SetValue(ConverterProperty, value);
        }

        protected override void OnAttachedTo(SelectableItemsView bindable)
        {
            base.OnAttachedTo(bindable);

            if (bindable.SelectionMode != SelectionMode.Single)
            {
                bindable.SelectionMode = SelectionMode.Single;
            }

            bindable.SelectionChanged += this.OnSelectionChanged;
        }

        protected override void OnDetachingFrom(SelectableItemsView bindable)
        {
            bindable.SelectionChanged -= this.OnSelectionChanged;
            base.OnDetachingFrom(bindable);
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // Ignore the change raised by clearing the selection below.
            if (this.isClearing || sender is not SelectableItemsView itemsView)
            {
                return;
            }

            var selectedItem = e.CurrentSelection?.FirstOrDefault();
            if (selectedItem == null)
            {
                return;
            }

            try
            {
                this.ExecuteCommand(selectedItem);
            }
            finally
            {
                // Clear the selection so the same item can be tapped again.
                this.isClearing = true;
                try
                {
                    itemsView.SelectedItem = null;
                }
                finally
                {
                    this.isClearing = false;
                }
            }
        }

        private void ExecuteCommand(object selectedItem)
        {
            if (this.Command is not ICommand command || this.AssociatedObject?.BindingContext == null)
            {
                return;
            }

            var parameter = this.CommandParameter ?? selectedItem;

            if (this.Converter is IValueConverter converter)
            {
                parameter = converter.Convert(parameter, typeof(object), null, CultureInfo.CurrentUICulture);
            }

            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }
}
