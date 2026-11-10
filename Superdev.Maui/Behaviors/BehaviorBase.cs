namespace Superdev.Maui.Behaviors
{
    public class BehaviorBase<T> : Behavior<T> where T : BindableObject
    {
        public T? AssociatedObject { get; private set; }

        protected override void OnAttachedTo(T bindable)
        {
            base.OnAttachedTo(bindable);
            this.AssociatedObject = bindable;

            if (bindable.BindingContext != null)
            {
                this.BindingContext = bindable.BindingContext;
            }

            bindable.BindingContextChanged += this.OnBindingContextChanged;
        }

        protected override void OnDetachingFrom(T bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.BindingContextChanged -= this.OnBindingContextChanged;
            this.AssociatedObject = null;
        }

        private void OnBindingContextChanged(object? sender, EventArgs e)
        {
            if (sender is BindableObject bindableObject)
            {
                // The behavior follows the binding context of its element, including back to null: a recycled
                // cell clears its context and gets a new one later, so the behavior has to stay attached.
                this.BindingContext = bindableObject.BindingContext;
            }
        }
    }
}