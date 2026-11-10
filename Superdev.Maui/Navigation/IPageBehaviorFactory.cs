namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Applies behaviors to every page the navigation service creates, e.g. in order to attach a behavior
    /// which every page of an application needs. Register an implementation in the service collection;
    /// several implementations are applied in registration order.
    /// </summary>
    /// <remarks>
    /// The factory runs after the page and its view model were created and the binding context was set,
    /// and before any lifecycle callback runs.
    /// </remarks>
    public interface IPageBehaviorFactory
    {
        /// <summary>
        /// Applies the behaviors of this factory to the given page.
        /// </summary>
        void ApplyBehaviors(Page page);
    }
}
