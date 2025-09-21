using Superdev.Maui.Extensions;

namespace Superdev.Maui.Behaviors
{
    /// <summary>
    /// Apply this behavior to an <seealso cref="Entry"/> in order to focus the next <seealso cref="VisualElement"/>
    /// if the Entry's Completed event is raised. The next element can either be <see cref="TargetElement"/> (supports binding) or <see cref="TargetElementName"/> (static string).
    /// </summary>
    ///
    /// <example>
    /// <Entry Placeholder="Entry 1">
    ///     <Entry.Behaviors>
    ///         <behaviors:InputViewCompletedBehavior TargetElement="{x:Reference Entry2}" />
    /// </Entry.Behaviors >
    /// </Entry >
    ///
    /// <Entry Placeholder="Entry 2">
    ///     <Entry.Behaviors>
    ///         <behaviors:InputViewCompletedBehavior TargetElementName="Entry2" />
    /// </Entry.Behaviors >
    /// </Entry >
    /// </example>
    public class InputViewCompletedBehavior : BehaviorBase<VisualElement>
    {
        public static readonly BindableProperty TargetElementProperty =
            BindableProperty.Create(
                nameof(TargetElement),
                typeof(VisualElement),
                typeof(InputViewCompletedBehavior));

        public VisualElement TargetElement
        {
            get => (VisualElement)this.GetValue(TargetElementProperty);
            set => this.SetValue(TargetElementProperty, value);
        }

        public string TargetElementName { get; set; }

        protected override void OnAttachedTo(VisualElement bindable)
        {
            base.OnAttachedTo(bindable);

            if (bindable.AsEntry() is Entry entry)
            {
                entry.Completed += this.OnEntryCompleted;
            }
            else if (bindable.AsEditor() is Editor editor)
            {
                editor.Completed += this.OnEntryCompleted;
            }
            else
            {
                throw new InvalidOperationException("bindable must be an Entry, Editor, or a subclass of either");
            }
        }

        protected override void OnDetachingFrom(VisualElement bindable)
        {
            if (bindable.AsEntry() is Entry entry)
            {
                entry.Completed -= this.OnEntryCompleted;
            }
            else if (bindable.AsEditor() is Editor editor)
            {
                editor.Completed -= this.OnEntryCompleted;
            }

            base.OnDetachingFrom(bindable);
        }

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.TargetElementName))
            {
                if (this.TargetElement is not VisualElement targetElement)
                {
                    throw new ArgumentNullException(nameof(this.TargetElement));
                }

                if (targetElement.AsEntry() is Entry entry)
                {
                    entry.Focus();
                }
                else if (targetElement.AsEditor() is Editor editor)
                {
                    editor.Focus();
                }
                else
                {
                    throw new InvalidOperationException("TargetElement must be an Entry, Editor, or a subclass of either");
                }
            }
            else
            {
                var parent = ((Element)sender).Parent;
                while (parent != null)
                {
                    var targetElement = parent.FindByName<VisualElement>(this.TargetElementName);
                    if (targetElement.AsEntry() is Entry entry)
                    {
                        entry.Focus();
                        break;
                    }

                    if (targetElement.AsEditor() is Editor editor)
                    {
                        editor.Focus();
                        break;
                    }

                    parent = parent.Parent;
                }
            }
        }
    }
}
