using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace Superdev.Maui.Tests.Navigation.TestDoubles
{
    /// <summary>
    /// Records the lifecycle callbacks of all test pages and view models of a test run.
    /// </summary>
    internal sealed class NavigationCallLog
    {
        public List<string> Entries { get; } = new List<string>();

        public void Add(object source, string callback)
        {
            this.Entries.Add($"{source.GetType().Name}.{callback}");
        }
    }

    /// <summary>
    /// A service which is registered as scoped, in order to verify that a page and its view model
    /// share one service scope, and that the scope is disposed with the page.
    /// </summary>
    internal sealed class ScopedService : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.IsDisposed = true;
        }
    }

    internal class TestParameter
    {
        public required string Value { get; init; }
    }

    internal sealed class DerivedTestParameter : TestParameter
    {
    }

    internal sealed class TestResult
    {
        public required string Value { get; init; }
    }

    internal abstract class TestViewModelBase : IInitialize, INavigatedTo, INavigatedFrom, IDisposable
    {
        protected TestViewModelBase(NavigationCallLog callLog, ScopedService scopedService)
        {
            this.CallLog = callLog;
            this.ScopedService = scopedService;
        }

        public NavigationCallLog CallLog { get; }

        public ScopedService ScopedService { get; }

        public bool IsDisposed { get; private set; }

        public virtual Task InitializeAsync()
        {
            this.CallLog.Add(this, nameof(this.InitializeAsync));
            return Task.CompletedTask;
        }

        public virtual Task NavigatedToAsync(NavigationDirection navigationDirection)
        {
            this.CallLog.Add(this, $"{nameof(this.NavigatedToAsync)}({navigationDirection})");
            return Task.CompletedTask;
        }

        public virtual Task NavigatedFromAsync(NavigationDirection navigationDirection)
        {
            this.CallLog.Add(this, $"{nameof(this.NavigatedFromAsync)}({navigationDirection})");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.IsDisposed = true;
            this.CallLog.Add(this, nameof(this.Dispose));
        }
    }

    internal sealed class HomeViewModel : TestViewModelBase
    {
        public HomeViewModel(NavigationCallLog callLog, ScopedService scopedService)
            : base(callLog, scopedService)
        {
        }
    }

    /// <summary>
    /// Receives a typed navigation parameter and a typed back navigation result.
    /// </summary>
    internal sealed class DetailViewModel : TestViewModelBase, IInitialize<TestParameter>, INavigatedTo<TestResult>
    {
        public DetailViewModel(NavigationCallLog callLog, ScopedService scopedService)
            : base(callLog, scopedService)
        {
        }

        public TestParameter? ReceivedParameter { get; private set; }

        public TestResult? ReceivedResult { get; private set; }

        public Task InitializeAsync(TestParameter parameter)
        {
            this.ReceivedParameter = parameter;
            this.CallLog.Add(this, $"{nameof(this.InitializeAsync)}({parameter.Value})");
            return Task.CompletedTask;
        }

        public Task NavigatedToAsync(NavigationDirection navigationDirection, TestResult parameter)
        {
            this.ReceivedResult = parameter;
            this.CallLog.Add(this, $"{nameof(this.NavigatedToAsync)}({navigationDirection}, {parameter.Value})");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Vetoes back navigation until <see cref="CanNavigate"/> is set.
    /// </summary>
    internal sealed class GuardedViewModel : TestViewModelBase, IConfirmNavigation
    {
        public GuardedViewModel(NavigationCallLog callLog, ScopedService scopedService)
            : base(callLog, scopedService)
        {
        }

        public bool CanNavigate { get; set; }

        /// <summary>
        /// Holds the answer until the returned task completes, like a guard which shows a dialog does.
        /// </summary>
        public Func<Task>? Hold { get; set; }

        public async Task<bool> CanNavigateAsync()
        {
            this.CallLog.Add(this, $"{nameof(this.CanNavigateAsync)}({this.CanNavigate})");

            if (this.Hold is { } hold)
            {
                await hold();
            }

            return this.CanNavigate;
        }
    }

    internal sealed class ResultViewModel : TestViewModelBase
    {
        public ResultViewModel(NavigationCallLog callLog, ScopedService scopedService)
            : base(callLog, scopedService)
        {
        }
    }

    /// <summary>
    /// Allows a test to run code from within a lifecycle callback of <see cref="RedirectViewModel"/>.
    /// </summary>
    internal sealed class NavigationHook
    {
        public Func<Task>? OnInitialize { get; set; }

        public Func<Task>? OnNavigatedTo { get; set; }
    }

    internal sealed class RedirectViewModel : TestViewModelBase
    {
        private readonly NavigationHook navigationHook;

        public RedirectViewModel(NavigationCallLog callLog, ScopedService scopedService, NavigationHook navigationHook)
            : base(callLog, scopedService)
        {
            this.navigationHook = navigationHook;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            if (this.navigationHook.OnInitialize is { } onInitialize)
            {
                await onInitialize();
            }
        }

        public override async Task NavigatedToAsync(NavigationDirection navigationDirection)
        {
            await base.NavigatedToAsync(navigationDirection);

            if (this.navigationHook.OnNavigatedTo is { } onNavigatedTo)
            {
                this.navigationHook.OnNavigatedTo = null;
                await onNavigatedTo();
            }
        }
    }

    internal sealed class RedirectPage : TestPageBase
    {
        public RedirectPage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }

    /// <summary>
    /// Verifies the <see cref="BaseViewModel"/> integration: the initialization callback is invoked
    /// by the navigation service and <see cref="BaseViewModel.IsInitialized"/> is set afterwards.
    /// </summary>
    internal sealed class BaseViewModelBackedViewModel : BaseViewModel, IInitialize<TestParameter>
    {
        private readonly NavigationCallLog callLog;

        public BaseViewModelBackedViewModel(NavigationCallLog callLog)
        {
            this.callLog = callLog;
        }

        public TestParameter? ReceivedParameter { get; private set; }

        public bool WasInitialized { get; private set; }

        protected override Task InitializeAsync()
        {
            this.WasInitialized = true;
            this.callLog.Add(this, nameof(this.InitializeAsync));
            return Task.CompletedTask;
        }

        public Task InitializeAsync(TestParameter parameter)
        {
            this.ReceivedParameter = parameter;
            this.callLog.Add(this, $"{nameof(this.InitializeAsync)}({parameter.Value})");
            return Task.CompletedTask;
        }
    }

    internal sealed class BaseViewModelBackedPage : TestPageBase
    {
        public BaseViewModelBackedPage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }

    internal sealed class LifecycleAwareViewModel : TestViewModelBase, IApplicationResumed, IApplicationStopped
    {
        public LifecycleAwareViewModel(NavigationCallLog callLog, ScopedService scopedService)
            : base(callLog, scopedService)
        {
        }

        public Task OnResumedAsync()
        {
            this.CallLog.Add(this, nameof(this.OnResumedAsync));
            return Task.CompletedTask;
        }

        public Task OnStoppedAsync()
        {
            this.CallLog.Add(this, nameof(this.OnStoppedAsync));
            return Task.CompletedTask;
        }
    }

    internal sealed class LifecycleAwarePage : TestPageBase
    {
        public LifecycleAwarePage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }

    /// <summary>
    /// A flyout page whose detail page is supplied by the navigation service.
    /// </summary>
    internal sealed class ShellFlyoutPage : FlyoutPage
    {
        public ShellFlyoutPage()
        {
            this.Flyout = new ContentPage { Title = "Menu" };
        }
    }

    /// <summary>
    /// Receives the parameters of a deep link query string.
    /// </summary>
    internal sealed class DeepLinkViewModel : TestViewModelBase, IInitialize<NavigationQueryParameters>
    {
        public DeepLinkViewModel(NavigationCallLog callLog, ScopedService scopedService)
            : base(callLog, scopedService)
        {
        }

        public NavigationQueryParameters? ReceivedParameters { get; private set; }

        public Task InitializeAsync(NavigationQueryParameters parameter)
        {
            this.ReceivedParameters = parameter;
            this.CallLog.Add(this, $"{nameof(this.InitializeAsync)}({parameter})");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// A second page which receives query parameters, so that per-segment delivery can be asserted.
    /// </summary>
    internal sealed class DeepLinkChildViewModel : TestViewModelBase, IInitialize<NavigationQueryParameters>
    {
        public DeepLinkChildViewModel(NavigationCallLog callLog, ScopedService scopedService)
            : base(callLog, scopedService)
        {
        }

        public NavigationQueryParameters? ReceivedParameters { get; private set; }

        public Task InitializeAsync(NavigationQueryParameters parameter)
        {
            this.ReceivedParameters = parameter;
            this.CallLog.Add(this, $"{nameof(this.InitializeAsync)}({parameter})");
            return Task.CompletedTask;
        }
    }

    internal sealed class DeepLinkChildPage : TestPageBase
    {
        public DeepLinkChildPage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }

    internal sealed class DeepLinkPage : TestPageBase
    {
        public DeepLinkPage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }

    /// <summary>
    /// A tabbed page whose tabs are declared statically, like a tabbed page declared in XAML.
    /// </summary>
    internal sealed class StaticTabbedPage : TabbedPage
    {
        public StaticTabbedPage(NavigationCallLog callLog, ScopedService scopedService)
        {
            this.Children.Add(new HomePage(scopedService)
            {
                BindingContext = new HomeViewModel(callLog, scopedService)
            });

            this.Children.Add(new NavigationPage(new DetailPage(scopedService)
            {
                BindingContext = new DetailViewModel(callLog, scopedService)
            }));
        }
    }

    internal sealed class ContextMenuViewModel : TestViewModelBase, IInitialize<TestParameter>
    {
        public ContextMenuViewModel(NavigationCallLog callLog, ScopedService scopedService)
            : base(callLog, scopedService)
        {
        }

        public TestParameter? ReceivedParameter { get; private set; }

        public Task InitializeAsync(TestParameter parameter)
        {
            this.ReceivedParameter = parameter;
            this.CallLog.Add(this, $"{nameof(this.InitializeAsync)}({parameter.Value})");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// A popup page like the context menus of the production apps: a dimmed background with a
    /// bottom sheet which closes when the background is tapped.
    /// </summary>
    internal sealed class ContextMenuPopupPage : Superdev.Maui.Controls.PopupPage
    {
        public ContextMenuPopupPage(ScopedService scopedService)
        {
            this.ScopedService = scopedService;
            this.BackgroundColor = Color.FromArgb("#80000000");
            this.Content = new VerticalStackLayout { VerticalOptions = LayoutOptions.End };
        }

        public ScopedService ScopedService { get; }

        /// <summary>
        /// Simulates the user tapping the background of the popup.
        /// </summary>
        public void SimulateBackgroundClick()
        {
            this.RaiseBackgroundClicked();
        }
    }

    /// <summary>
    /// A popup which the user must not close by tapping the background.
    /// </summary>
    internal sealed class ModalPopupPage : Superdev.Maui.Controls.PopupPage
    {
        public ModalPopupPage(ScopedService scopedService)
        {
            this.ScopedService = scopedService;
            this.CloseWhenBackgroundIsClicked = false;
            this.Content = new VerticalStackLayout();
        }

        public ScopedService ScopedService { get; }

        public void SimulateBackgroundClick()
        {
            this.RaiseBackgroundClicked();
        }
    }

    /// <summary>
    /// Fails during the one-shot initialization, i.e. after its page was created but before the page
    /// reached the navigation stack.
    /// </summary>
    internal sealed class ThrowingInitializeViewModel : TestViewModelBase
    {
        public ThrowingInitializeViewModel(NavigationCallLog callLog, ScopedService scopedService)
            : base(callLog, scopedService)
        {
        }

        public override Task InitializeAsync()
        {
            base.InitializeAsync();
            throw new InvalidOperationException("InitializeAsync failed");
        }
    }

    internal sealed class ThrowingInitializePage : TestPageBase
    {
        public ThrowingInitializePage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }

    /// <summary>
    /// Fails inside the typed initialization callback before its first await, which is the case reflection
    /// would report as a <see cref="System.Reflection.TargetInvocationException"/>.
    /// </summary>
    internal sealed class ThrowingTypedInitializeViewModel : TestViewModelBase, IInitialize<TestParameter>
    {
        public ThrowingTypedInitializeViewModel(NavigationCallLog callLog, ScopedService scopedService)
            : base(callLog, scopedService)
        {
        }

        public Task InitializeAsync(TestParameter parameter)
        {
            throw new InvalidOperationException("Typed InitializeAsync failed");
        }
    }

    internal sealed class ThrowingTypedInitializePage : TestPageBase
    {
        public ThrowingTypedInitializePage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }

    /// <summary>
    /// A modal page which asks to be presented as an iOS sheet, as a page of an application might.
    /// </summary>
    internal sealed class SheetPage : TestPageBase
    {
        public SheetPage(ScopedService scopedService)
            : base(scopedService)
        {
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetModalPresentationStyle(
                this.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>(),
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.PageSheet);
        }
    }

    internal sealed class SheetViewModel : TestViewModelBase
    {
        public SheetViewModel(NavigationCallLog callLog, ScopedService scopedService)
            : base(callLog, scopedService)
        {
        }
    }

    /// <summary>
    /// A sheet page whose view model carries a navigation guard.
    /// </summary>
    internal sealed class GuardedSheetPage : TestPageBase
    {
        public GuardedSheetPage(ScopedService scopedService)
            : base(scopedService)
        {
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetModalPresentationStyle(
                this.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>(),
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.PageSheet);
        }
    }

    /// <summary>
    /// Fails when it is navigated back to, i.e. after the page which was popped already left the stack.
    /// </summary>
    internal sealed class ThrowingNavigatedToViewModel : TestViewModelBase
    {
        public ThrowingNavigatedToViewModel(NavigationCallLog callLog, ScopedService scopedService)
            : base(callLog, scopedService)
        {
        }

        public override Task NavigatedToAsync(NavigationDirection navigationDirection)
        {
            base.NavigatedToAsync(navigationDirection);

            if (navigationDirection == NavigationDirection.Back)
            {
                throw new InvalidOperationException("NavigatedToAsync failed");
            }

            return Task.CompletedTask;
        }
    }

    internal sealed class ThrowingNavigatedToPage : TestPageBase
    {
        public ThrowingNavigatedToPage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }

    /// <summary>
    /// Neither this page nor its view model is registered, so both are resolved by convention and created
    /// outside the service container.
    /// </summary>
    internal sealed class ConventionOnlyPage : ContentPage
    {
    }

    /// <inheritdoc cref="ConventionOnlyPage"/>
    internal sealed class ConventionOnlyViewModel : IDisposable
    {
        private readonly NavigationCallLog callLog;

        public ConventionOnlyViewModel(NavigationCallLog callLog)
        {
            this.callLog = callLog;
        }

        public void Dispose()
        {
            this.callLog.Add(this, nameof(this.Dispose));
        }
    }

    /// <summary>
    /// Lets a test hold a navigation inside a lifecycle callback, so that a second navigation has to queue.
    /// </summary>
    internal sealed class NavigationGate
    {
        private TaskCompletionSource? gate;

        public void Close()
        {
            this.gate = new TaskCompletionSource();
        }

        public void Open()
        {
            this.gate?.TrySetResult();
        }

        public Task WaitAsync()
        {
            return this.gate?.Task ?? Task.CompletedTask;
        }
    }

    internal sealed class GatedViewModel : TestViewModelBase
    {
        private readonly NavigationGate gate;

        public GatedViewModel(NavigationCallLog callLog, ScopedService scopedService, NavigationGate gate)
            : base(callLog, scopedService)
        {
            this.gate = gate;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.gate.WaitAsync();
        }
    }

    internal sealed class GatedPage : TestPageBase
    {
        public GatedPage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }

    internal abstract class TestPageBase : ContentPage
    {
        protected TestPageBase(ScopedService scopedService)
        {
            this.ScopedService = scopedService;
        }

        public ScopedService ScopedService { get; }
    }

    internal sealed class HomePage : TestPageBase
    {
        public HomePage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }

    internal sealed class DetailPage : TestPageBase
    {
        public DetailPage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }

    internal sealed class GuardedPage : TestPageBase
    {
        public GuardedPage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }

    internal sealed class ResultPage : TestPageBase
    {
        public ResultPage(ScopedService scopedService)
            : base(scopedService)
        {
        }
    }
}
