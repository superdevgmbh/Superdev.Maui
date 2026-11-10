using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Provides a command which navigates to a registered page, so that a simple navigation does not need a
    /// command on the view model.
    /// </summary>
    /// <example>
    /// <code>&lt;Button Command="{s:NavigateTo DetailPage}" /&gt;</code>
    /// </example>
    /// <remarks>
    /// The command uses <see cref="INavigationService.Current"/>. A navigation which needs a parameter from
    /// the view model, or which has to react to its outcome, belongs on the view model instead.
    /// </remarks>
    [ContentProperty(nameof(PageName))]
    public class NavigateToExtension : IMarkupExtension<ICommand>
    {
        /// <summary>
        /// The navigation name of the page to navigate to.
        /// </summary>
        public string? PageName { get; set; }

        /// <summary>
        /// Presents the page as a modal page.
        /// </summary>
        public bool Modal { get; set; }

        /// <summary>
        /// Animates the transition.
        /// </summary>
        public bool Animated { get; set; } = true;

        /// <inheritdoc />
        public ICommand ProvideValue(IServiceProvider serviceProvider)
        {
            var pageName = this.PageName;
            var modal = this.Modal;
            var animated = this.Animated;

            return new NavigationCommand(() =>
            {
                if (string.IsNullOrWhiteSpace(pageName))
                {
                    throw new NavigationException($"{nameof(NavigateToExtension)} requires a page name");
                }

                return modal
                    ? INavigationService.Current.PushModalAsync(pageName, animated)
                    : INavigationService.Current.PushAsync(pageName, animated);
            });
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return this.ProvideValue(serviceProvider);
        }
    }

    /// <summary>
    /// Provides a command which navigates back.
    /// </summary>
    /// <example>
    /// <code>&lt;Button Command="{s:GoBack}" /&gt;</code>
    /// </example>
    public class GoBackExtension : IMarkupExtension<ICommand>
    {
        /// <summary>
        /// Navigates back to the root page of the navigation stack instead of the previous page.
        /// </summary>
        public bool ToRoot { get; set; }

        /// <summary>
        /// Animates the transition.
        /// </summary>
        public bool Animated { get; set; } = true;

        /// <inheritdoc />
        public ICommand ProvideValue(IServiceProvider serviceProvider)
        {
            var toRoot = this.ToRoot;
            var animated = this.Animated;

            return new NavigationCommand(() => toRoot
                ? INavigationService.Current.PopToRootAsync(animated)
                : INavigationService.Current.PopAsync(animated));
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return this.ProvideValue(serviceProvider);
        }
    }

    /// <summary>
    /// Runs a navigation from XAML. A navigation which a guard vetoed is not an error, and any other failure
    /// is logged rather than thrown, because a command has no caller which could observe it.
    /// </summary>
    internal sealed class NavigationCommand : ICommand
    {
        private readonly Func<Task> navigate;
        private bool isNavigating;

        public NavigationCommand(Func<Task> navigate)
        {
            this.navigate = navigate;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return !this.isNavigating;
        }

        public async void Execute(object? parameter)
        {
            if (this.isNavigating)
            {
                return;
            }

            this.isNavigating = true;
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);

            try
            {
                await this.navigate();
            }
            catch (NavigationCancelledException)
            {
                // A navigation guard vetoed the navigation.
            }
            catch (Exception ex)
            {
                var logger = IPlatformApplication.Current?.Services.GetService<ILogger<NavigationCommand>>();
                logger?.LogError(ex, "The navigation of a XAML command failed with exception");
            }
            finally
            {
                this.isNavigating = false;
                this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
