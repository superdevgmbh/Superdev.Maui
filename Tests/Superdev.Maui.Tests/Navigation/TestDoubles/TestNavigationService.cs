using Superdev.Maui.Navigation;

namespace Superdev.Maui.Tests.Navigation.TestDoubles
{
    /// <summary>
    /// Records the confirmed back navigation from the root page instead of closing the application,
    /// which a unit test process must not do.
    /// </summary>
    internal sealed class TestNavigationService : NavigationService
    {
        public TestNavigationService(NavigationServiceContext context)
            : base(context)
        {
        }

        private readonly TaskCompletionSource backFromRootPageConfirmed = new TaskCompletionSource();

        public int BackFromRootPageConfirmedCount { get; private set; }

        /// <summary>
        /// Completes once the back navigation from the root page was confirmed for the first time, so that a
        /// test can wait for a confirmation which a guard held back.
        /// </summary>
        public Task BackFromRootPageConfirmed => this.backFromRootPageConfirmed.Task;

        protected override Task OnBackFromRootPageConfirmedAsync()
        {
            this.BackFromRootPageConfirmedCount++;
            this.backFromRootPageConfirmed.TrySetResult();
            return Task.CompletedTask;
        }
    }
}
