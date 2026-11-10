# Superdev.Maui
[![Version](https://img.shields.io/nuget/v/Superdev.Maui.svg)](https://www.nuget.org/packages/Superdev.Maui) [![Downloads](https://img.shields.io/nuget/dt/Superdev.Maui.svg)](https://www.nuget.org/packages/Superdev.Maui) [![Buy Me a Coffee](https://img.shields.io/badge/support-buy%20me%20a%20coffee-FFDD00)](https://buymeacoffee.com/thomasgalliker)

Superdev.Maui is a comprehensive support library for .NET MAUI applications, providing production-ready building blocks such as boilerplate infrastructure, cross-cutting services, reusable tools, and custom controls. It streamlines common mobile app development tasks by offering consistent patterns, commonly used abstractions, and ready-to-use components designed to accelerate development and improve maintainability across MAUI projects.

### Download and Install Superdev.Maui
This library is available on NuGet: https://www.nuget.org/packages/Superdev.Maui
Use the following command to install Superdev.Maui using NuGet package manager console:

    PM> Install-Package Superdev.Maui

Superdev.Maui currently targets .NET 9 and .NET 10 and runs on iOS 12.2+ and Android API 24+.

### Setup
**1. Register the library** in your `MauiProgram.cs` by calling `UseSuperdevMaui(...)` on the `MauiAppBuilder`. The optional `SuperdevMauiOptions` let you plug in a translation provider, opt into ignoring the safe area, or toggle automatic page cleanup:

```csharp
var builder = MauiApp.CreateBuilder();
builder
    .UseMauiApp<App>()
    .UseSuperdevMaui(o =>
    {
        o.TranslationProvider = new ResxTranslationProvider(Strings.ResourceManager);
        // o.IgnoreSafeArea = false;   // default
        // o.AutoCleanupPage = true;   // default
    });
```

This registers all platform handlers, effects, and the library's services (dialogs, navigation, preferences, localization, theming, and more) into the DI container.

**2. Add the XAML namespace** to your pages so you can reference Superdev.Maui controls, effects, behaviors, and markup extensions. Throughout this README and the sample app the `s` prefix is used:

```xml
xmlns:s="http://schemas.superdev.ch/dotnet/2021/maui"
```

**3. Merge the styles and define a theme** in your `App.xaml`. Merge `SuperdevMauiStyles` and configure a `Theme` from a `ColorConfiguration`, `FontConfiguration`, and `SpacingConfiguration`:

```xml
<Application
    xmlns:s="http://schemas.superdev.ch/dotnet/2021/maui"
    xmlns:styles="clr-namespace:Superdev.Maui.Resources.Styles;assembly=Superdev.Maui">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <styles:SuperdevMauiStyles />
            </ResourceDictionary.MergedDictionaries>

            <s:ColorConfiguration
                x:Key="ColorConfigurationLight"
                Error="#ff1744"
                OnPrimary="#FFFFFF"
                Primary="#5714AF"
                Secondary="#17C2BC" />

            <s:Theme
                x:Key="App.Theme.Light"
                ColorConfiguration="{StaticResource ColorConfigurationLight}" />
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### API Usage
The sections below show the most-used building blocks. Every feature has a runnable example in the [sample app](Samples/SuperdevMauiDemoApp).

#### Controls
Superdev.Maui ships enhanced versions of the standard MAUI controls (`CustomEntry`, `CustomLabel`, `CustomButton`, `CustomEditor`, `CustomPicker`, `CustomScrollView`, `CustomWebView`, ...), a set of validation-aware controls (`ValidatableEntry`, `ValidatableEditor`, `ValidatablePicker`, `ValidatableDatePicker`), `NullableDatePicker` / `NullableTimePicker`, a `CardView` system, swipe items, activity indicators, and drilldown controls.

A `ValidatableEntry` inside a `CardView`, wired to validation errors and input behaviors:

```xml
<s:CardView>
    <VerticalStackLayout>
        <s:LabelSection Text="ValidatableEntry" />
        <s:HeaderDivider />
        <s:ValidatableEntry
            Placeholder="ValidatableEntry"
            Text="{Binding UserName, Mode=TwoWay}"
            ValidationErrors="{Binding Validation.Errors[UserName]}">
            <s:ValidatableEntry.Behaviors>
                <s:InputViewMaxLengthBehavior MaxLength="{Binding UserNameMaxLength}" />
                <s:InputViewUnfocusedBehavior DecorationFlags="Trim" />
            </s:ValidatableEntry.Behaviors>
        </s:ValidatableEntry>
        <s:FooterDivider />
    </VerticalStackLayout>
</s:CardView>
```

See the **EntryDemoPage**, **CardViewDemoPage**, **DatePickerDemoPage**, and other demos for the full set.

#### MVVM
`BaseViewModel` and `BindableBase` provide the MVVM foundation: `INotifyPropertyChanged` via `SetProperty`, busy/refresh state (`IsBusy`, `IsRefreshing`, `RefreshCommand`), and a built-in `ViewModelError` channel. `CommandGroup` serializes command execution so overlapping invocations are ignored.

```csharp
public class MyViewModel : BaseViewModel
{
    private string? userName;

    public string? UserName
    {
        get => this.userName;
        set => this.SetProperty(ref this.userName, value);
    }
}
```

#### Validation
`ViewModelValidation` is the form state of a view model: the rules, the errors they produce, errors set from anywhere else, and per field whether the user has touched it, changed it, or is waiting for an asynchronous rule. Declare the rules by overriding `SetupValidation`:

```csharp
protected override ViewModelValidation SetupValidation()
{
    var validation = new ViewModelValidation();

    validation.Property(() => this.UserName)
        .NotEmpty()
        .MinimumLength(3)
        .MustAsync(this.userService.IsUserNameAvailableAsync, debounce: TimeSpan.FromMilliseconds(400));

    validation.Property(() => this.Email)
        .NotEmpty()
        .EmailAddress()
        .Alias("DuplicateEmail");                     // an incoming error key this field owns

    validation.Property(() => this.PasswordConfirm)
        .NotEmpty()
        .EqualTo(() => this.Password);                // validated again when the password changes

    validation.Property(() => this.Notes)
        .NotEmpty().InGroup("Submit");                // runs only when the "Submit" group is selected

    validation.Include(nameof(this.Address), () => this.Address?.Validation);   // a child view model's form

    validation.Property(() => this.CustomerNumber);   // no rules: still receives errors set from outside

    return validation;
}
```

Wire a control with one attribute. The library's own controls render the message; any other control gets blur handling, focus on the first error and a screen-reader hint, and `ValidationErrorLabel` shows the message next to it:

```xml
<s:ValidatableEntry Text="{Binding UserName}" s:Validation.PropertyName="UserName" />

<Entry Text="{Binding Nickname}" s:Validation.PropertyName="Nickname" />
<s:ValidationErrorLabel PropertyName="Nickname" />

<s:ValidationErrorBox Validation="{Binding Validation}" />   <!-- every message, children's included -->
```

Validate explicitly, or validate and act — the save, the API call — in one loop. A result which did not succeed lands on the fields; a key nobody owns is kept under `Errors.Form` rather than dropped:

```csharp
await this.Validation.ValidateAsync(async ct =>
{
    var response = await this.personService.SaveAsync(this.ToDto(), ct);
    return response.IsSuccess
        ? ValidationResult.Success
        : ValidationResult.FromErrors(response.Errors);   // any property → messages map
});
```

A field the user edits while `ValidateAsync` runs is judged again before the verdict, so the action never sees a value its rules have not passed. A successful action clears every error set from outside, in child forms too; a plain `ValidateAsync()` keeps them and returns `false` while they stand. An included form (`Include`) follows its parent's `Mode`, `ReValidateMode`, clock and message provider unless it sets its own, runs the parent's active groups, and its changes show in the parent's `IsValid`, `HasErrors` and `Errors.All` at once.

Groups selected for a validation stay active for the re-validation that follows: `ValidateAsync(groups: ["Submit"])` keeps "Submit" active until another call changes it, and `ValidateAsync()` keeps whatever is active. Pass an empty list to run the ungrouped rules only.

`Mode` decides when a field validates for the first time — `Manual` (the default: only an explicit `ValidateAsync`), `OnBlur`, `OnTouched`, `OnChange`, `All` — and `ReValidateMode` when a field with an error, or any field once the form was validated, validates again (`OnChange` by default, so the error clears the moment it is corrected). Synchronous rules report at once; an asynchronous rule runs only when they all pass, the field is pending meanwhile, and its verdict counts for `IsValid` until the value changes. A `When` condition that turns off takes its errors with it. `Validation.Errors[UserName]`, `Errors.Form`, `Errors.All`, `Fields[UserName].IsTouched` / `IsDirty` / `IsPending`, `IsValid`, `IsDirty`, `IsValidating` and `HasValidated` are bindable. `IsDirty` compares with the values at the first access to `Validation`: call `Validation.Reset()` once the view model has loaded its data. A rule which throws while a field is validated after a change is reported through the `ValidationFailed` event.

Rules: `NotNull`, `NotEmpty`, `IsTrue`, `EqualTo`, `NotEqualTo`, `MinimumLength`, `MaximumLength`, `Length`, `Matches`, `EmailAddress`, `GreaterThan[OrEqualTo]`, `LessThan[OrEqualTo]`, `Between`, `OneOf`, `InThePast`, `InTheFuture`, `IsInteger`, `IsDecimal`, `IsDate`, `IsTime`, `Must`, `MustAsync`. Chain verbs: `When`, `Unless`, `Alias`, `WithMessage`, `WithErrorCode`, `WithDisplayName`, `InGroup` / `InGroups`, `ValidateOn`. A missing value — `null`, or text that is empty — passes every rule except `NotNull`, `NotEmpty` and `IsTrue`; `EqualTo(() => other)` passes while either side is missing. A chain stops at its first failing rule. A collection that announces its changes (`ObservableCollection`) is watched: adding or removing an item validates the field, and `IsDirty` compares its items. After changing any other collection in place, raise `PropertyChanged` for its property. `IsDecimal` parses in the current culture: `1,5` under de-DE, `1.5` under en-US and de-CH.

Messages resolve through `IValidationMessageProvider`. `UseSuperdevMaui` installs one backed by the app's `ITranslationProvider`: it looks up `Validation_{ErrorCode}` for the message (e.g. `Validation_NotEmpty` = `{PropertyName} darf nicht leer sein.`) and `ValidationField_{PropertyName}` for the name of a field (e.g. `ValidationField_PostalCode` = `Postleitzahl`), and falls back to English and to the property name split into words. `WithDisplayName` and `WithMessage` take precedence.

The error codes are the constants of `ValidationErrorCodes`. Every message gets `{PropertyName}`, the display name of the field; the table lists what each code adds. An own `IValidationMessageProvider` receives the same code and arguments in `GetMessage(errorCode, arguments)`.

| Translation key | Rules | Arguments | Default English text |
|---|---|---|---|
| `Validation_NotNull` | `NotNull` | — | `{PropertyName} must not be empty.` |
| `Validation_NotEmpty` | `NotEmpty` | — | `{PropertyName} must not be empty.` |
| `Validation_IsTrue` | `IsTrue` | — | `{PropertyName} is required.` |
| `Validation_EqualTo` | `EqualTo(value)` | `ComparisonValue` | `{PropertyName} must be equal to {ComparisonValue}.` |
| `Validation_NotEqualTo` | `NotEqualTo(value)` | `ComparisonValue` | `{PropertyName} must not be equal to {ComparisonValue}.` |
| `Validation_EqualToOtherProperty` | `EqualTo(() => Other)` | `OtherPropertyName` | `{PropertyName} must match {OtherPropertyName}.` |
| `Validation_NotEqualToOtherProperty` | `NotEqualTo(() => Other)` | `OtherPropertyName` | `{PropertyName} must not match {OtherPropertyName}.` |
| `Validation_MinimumLength` | `MinimumLength` | `MinimumLength` | `{PropertyName} must be at least {MinimumLength} characters long.` |
| `Validation_MaximumLength` | `MaximumLength` | `MaximumLength` | `{PropertyName} must be at most {MaximumLength} characters long.` |
| `Validation_Length` | `Length` | `MinimumLength`, `MaximumLength` | `{PropertyName} must be between {MinimumLength} and {MaximumLength} characters long.` |
| `Validation_Matches` | `Matches` | `Pattern` | `{PropertyName} is not in the correct format.` |
| `Validation_EmailAddress` | `EmailAddress` | — | `{PropertyName} is not a valid e-mail address.` |
| `Validation_GreaterThan` | `GreaterThan` | `ComparisonValue` | `{PropertyName} must be greater than {ComparisonValue}.` |
| `Validation_GreaterThanOrEqualTo` | `GreaterThanOrEqualTo` | `ComparisonValue` | `{PropertyName} must be greater than or equal to {ComparisonValue}.` |
| `Validation_LessThan` | `LessThan` | `ComparisonValue` | `{PropertyName} must be less than {ComparisonValue}.` |
| `Validation_LessThanOrEqualTo` | `LessThanOrEqualTo` | `ComparisonValue` | `{PropertyName} must be less than or equal to {ComparisonValue}.` |
| `Validation_Between` | `Between` | `From`, `To` | `{PropertyName} must be between {From} and {To}.` |
| `Validation_OneOf` | `OneOf` | `Values` (comma-separated) | `{PropertyName} must be one of: {Values}.` |
| `Validation_InThePast` | `InThePast` | — | `{PropertyName} must be in the past.` |
| `Validation_InTheFuture` | `InTheFuture` | — | `{PropertyName} must be in the future.` |
| `Validation_IsInteger` | `IsInteger` | — | `{PropertyName} must be a whole number.` |
| `Validation_IsDecimal` | `IsDecimal` | — | `{PropertyName} must be a number.` |
| `Validation_IsDate` | `IsDate` | — | `{PropertyName} must be a valid date.` |
| `Validation_IsTime` | `IsTime` | — | `{PropertyName} must be a valid time.` |
| `Validation_Must` | `Must` | — | `{PropertyName} is not valid.` |
| `Validation_MustAsync` | `MustAsync` | — | `{PropertyName} is not valid.` |
| `Validation_Custom` | errors set from outside without a message (`SetErrors`, `ValidateAsync(action)`) | — | `{PropertyName} is not valid.` |

A code of your own (`WithErrorCode("PostalCodeUnknown")`) is looked up as `Validation_PostalCodeUnknown` and falls back to the `Custom` text. Field names are looked up as `ValidationField_{PropertyName}`.

#### Navigation
`INavigationService` abstracts push/pop, modal, root and multi-segment navigation. Register pages and view models with `RegisterForNavigation`:

```csharp
builder.Services.RegisterForNavigation<MainPage, MainViewModel>("MainPage");
builder.Services.RegisterForNavigation<EntryDemoPage, EntryDemoViewModel>();
```

A page which is not registered is resolved by scanning the loaded assemblies for a type of that name, and
its `{Name}ViewModel` counterpart; a warning is logged the first time this happens. A trimmed or ahead-of-time
compiled build cannot resolve pages that way, so register every page explicitly there and switch the scan
off with `NavigationOptions.EnableConventionBasedResolution = false`.

`UseSuperdevMaui()` registers `INavigationService` with `TryAddSingleton`, so an implementation of your own
takes precedence if it is registered before that call - or afterwards with `services.Replace(...)`.

Create the application's start window from a navigation URI, so that the root page and its view model
get their service scope and their lifecycle callbacks like any other page:

```csharp
protected override Window CreateWindow(IActivationState? activationState)
{
    return this.navigationService.CreateWindow("/NavigationPage/MainPage");
}
```

Navigate by view model type or by navigation name:

```csharp
await this.navigationService.PushAsync<EntryDemoViewModel>();
await this.navigationService.PushAsync("EntryDemoPage");
await this.navigationService.PushModalAsync("FilterPage");
await this.navigationService.PopAsync();
```

Multi-segment navigation composes a whole page tree in one call. A leading slash replaces the root page of
the window, otherwise the segments are pushed onto the current navigation stack:

```csharp
await this.navigationService.NavigateAsync("/NavigationPage/HomePage");           // new root
await this.navigationService.NavigateAsync("/MainPage/NavigationPage/HomePage");  // flyout page + detail
await this.navigationService.NavigateAsync("DetailPage/EditPage");                // push both
await this.navigationService.NavigateAsync("NavigationPage/SettingsPage");        // replaces the flyout detail
```

If the root page is a `FlyoutPage`, a relative navigation which starts with a navigation page replaces its
detail page - which is how a flyout menu switches between sections. The pages of the previous detail page
are asked for confirmation and are disposed afterwards.

A relative navigation can also remove pages before it pushes new ones. Each leading `..` segment removes
one page from the navigation stack:

```csharp
await this.navigationService.NavigateAsync("../DetailPage");      // replaces the current page
await this.navigationService.NavigateAsync("../../DetailPage");   // replaces the last two pages
```

**Deep links and app actions** are ordinary navigation URIs. Pass the link straight through, or map an app
action to a URI, and receive the query string as a `NavigationQueryParameters` parameter. An absolute URI
replaces the whole page tree, so the guards of the pages it removes are evaluated first - a link arriving
over unsaved changes asks before it discards them:

```csharp
protected override void OnAppLinkRequestReceived(Uri uri)
{
    _ = this.navigationService.NavigateAsync(uri.ToString());   // e.g. myapp://app/NavigationPage/DetailPage?id=42
}

AppActions.Current.AppActionActivated += (_, e) =>
    MainThread.BeginInvokeOnMainThread(async () =>
        await INavigationService.Current.NavigateAsync($"/NavigationPage/{e.AppAction.Id}"));
```

```csharp
public class DetailViewModel : BaseViewModel, IInitialize<NavigationQueryParameters>
{
    public Task InitializeAsync(NavigationQueryParameters parameters)
    {
        parameters.TryGetValue<int>("id", out var id);
        parameters.TryGetEnumValue<DetailMode>("mode", out var mode);
        return Task.CompletedTask;
    }
}
```

**Tabbed pages**: declare the tabs statically in XAML and select them programmatically. The lifecycle
callbacks are delivered to the active tab child only - both when the tab is selected through the navigation
service and when the user taps the tab bar:

```csharp
await this.navigationService.SelectTabAsync("SettingsPage");
await this.navigationService.SelectTabAsync<SettingsViewModel>();
```

Navigation failures throw a `NavigationException`; if a navigation method returns, the navigation happened.

**Removing pages.** Besides `PopAsync` and `PopToRootAsync`, the stack can be shortened in three more ways:

```csharp
await this.navigationService.PopToAsync("HomePage");          // back to a named page
await this.navigationService.PushAsRootAsync("HomePage");     // push and drop everything below it
await this.navigationService.RemovePageAsync(intermediatePage); // drop a page which is not visible
```

All three evaluate the navigation guards of the pages they remove and dispose their service scopes.

**Closing popups.** `PopAsync` closes the topmost popup. `PopAllPopupsAsync` closes all of them, topmost
first, each one through the full pipeline including its guard:

```csharp
await this.navigationService.PopAllPopupsAsync();
```

**Navigating from XAML.** For a navigation which needs neither a parameter from the view model nor a
reaction to its outcome, there are markup extensions:

```xml
<Button Command="{s:NavigateTo DetailPage}" Text="Details" />
<Button Command="{s:NavigateTo SettingsPage, Modal=True}" Text="Settings" />
<Button Command="{s:GoBack}" Text="Back" />
```

**Navigation parameters** are plain objects. Pass any object and receive it through a typed lifecycle interface:

```csharp
await this.navigationService.PushAsync("DetailPage", new DetailViewModel.NavigationParameter { Id = 42 });
```

```csharp
public class DetailViewModel : BaseViewModel, IInitialize<DetailViewModel.NavigationParameter>
{
    public Task InitializeAsync(NavigationParameter parameter) { /* ... */ }

    public class NavigationParameter
    {
        public required int Id { get; init; }
    }
}
```

**Lifecycle interfaces** — implement them on the view model, on the page, or on both:

| Interface | Called |
|---|---|
| `IInitialize` / `IInitialize<TParameter>` | once per page, before it becomes visible |
| `INavigatedTo` / `INavigatedTo<TParameter>` | every time the page becomes the active page |
| `INavigatedFrom` | every time the page stops being the active page |
| `IConfirmNavigation` | before a navigation which would remove the page, in order to veto it |
| `IApplicationResumed` / `IApplicationStopped` | when the application returns to the foreground or is sent to the background - delivered to **every** live page, not just the visible one |
| `IDisposable` | when the page left the navigation stack |

**Popups** are pages, not a separate concept. Derive from `PopupPage`, register it like any other page and
open it with the ordinary navigation calls - no additional package required:

```xml
<s:PopupPage
    x:Class="MyApp.Views.ContextMenuPopupPage"
    xmlns:s="http://schemas.superdev.ch/dotnet/2021/maui"
    BackgroundColor="#80000000"
    CloseWhenBackgroundIsClicked="True">
    <s:PopupPage.Animation>
        <s:MoveAnimation
            DurationIn="150"
            EasingIn="SinOut"
            PositionIn="Bottom"
            PositionOut="Bottom" />
    </s:PopupPage.Animation>

    <VerticalStackLayout VerticalOptions="End">
        <!-- the popup content -->
    </VerticalStackLayout>
</s:PopupPage>
```

```csharp
await this.navigationService.PushAsync("ContextMenuPopupPage", parameter);
var selection = await this.navigationService.PushForResultAsync<MenuItem>("ContextMenuPopupPage");
await this.navigationService.PopAsync();   // closes the popup
```

A popup is presented as a transparent modal page, so it covers the navigation bar and the tab bar. It gets
the same service scope, navigation parameters, lifecycle callbacks, navigation guards and result navigation
as any other page, and push/pop of ordinary pages still applies to the navigation stack underneath it.
Available animations: `FadeAnimation`, `MoveAnimation` and `ScaleAnimation`, all with `DurationIn`/`DurationOut`,
`EasingIn`/`EasingOut` and `PositionIn`/`PositionOut`.

**Navigation guards** let a page veto a navigation which would remove it:

```csharp
public class EditViewModel : BaseViewModel, IConfirmNavigation
{
    public Task<bool> CanNavigateAsync()
    {
        return this.dialogService.DisplayAlertAsync("Discard changes?", "...", "Discard", "Cancel");
    }
}
```

A navigation page declared in XAML - the tabs of a `TabbedPage`, typically - has to be `s:CustomNavigationPage`
rather than `NavigationPage` for the Android hardware back button to reach the guards; navigation URIs get it
automatically, because `CustomNavigationPage` is registered under the name `NavigationPage`.

A guard is asked whenever its page would be removed: by a pop, a modal dismissal, a flyout detail replacement,
and by an absolute navigation - a deep link - which replaces the whole page tree. Back navigation triggered by
the user is covered as well: the Android hardware back button and the back button of the iOS navigation bar
are routed through the navigation service, and a modal page which the user tries to dismiss is asked first. On iOS the interactive swipe back gesture cannot be
cancelled once it started, therefore it is disabled for pages with a guard, and a guarded modal page is
presented full screen rather than as a sheet which could be swiped away (see
`NavigationOptions.DisableSwipeBackOnGuardedPages` and `PresentGuardedModalPagesFullScreen`). A vetoed
navigation throws a `NavigationCancelledException`.

`BaseViewModel` already implements `IInitialize`: override its `InitializeAsync` method and `IsInitialized` (and with it `IsBusy` and `IsContentReady`) is set as soon as it completed. All other hooks are opt-in - implement only the interfaces you actually need, on the view model, on the page, or on both.

**Returning a result** from a page replaces the "pass a `TaskCompletionSource` around" pattern:

```csharp
var result = await this.navigationService.PushForResultAsync<SelectionResult>("SelectionPage");
// ... and in the pushed page's view model:
await this.navigationService.PopAsync(new SelectionResult { ... });
```

**Per-page service scopes**: every navigated page gets its own service scope. The page, its view model and every service registered with `AddScoped` share that scope, and the scope is disposed as soon as the page leaves the navigation stack — which disposes the view model and all scoped services deterministically.

#### Services
A range of injectable, testable services is registered automatically: `IDialogService`, `IToastService`, `IBrowser`, `IPreferences`, `IDateTime`, `IDeviceInfo`, `IStatusBarService`, `IActivityIndicatorService`, `IKeyboardService`, and `IMainThread`. Inject them through the constructor:

```csharp
public MyViewModel(IDialogService dialogService)
{
    this.dialogService = dialogService;
}

private async Task ConfirmAsync()
{
    var ok = await this.dialogService.DisplayAlertAsync("Title", "Message", "OK", "Cancel");
}
```

`IDialogService` also asks for a line of text and shows action sheets whose buttons carry their own role
and action, so that the caller does not have to compare the returned text:

```csharp
var name = await this.dialogService.DisplayPromptAsync("Name", "How should we call you?");

var pressed = await this.dialogService.DisplayActionSheetAsync(
    "Photo",
    ActionSheetButton.Create("Take a photo", this.TakePhotoAsync),
    ActionSheetButton.CreateDestroy("Delete", this.DeleteAsync),
    ActionSheetButton.CreateCancel("Cancel"));
```

The action of the pressed button has run by the time the call returns; the returned button is `null` when
the sheet was dismissed without a choice.

`IPreferences` adds typed, JSON-backed `Get<T>`/`Set<T>` over the platform key-value store. See the **ServiceDemoPage** and **PreferencesDemoPage**.

#### Localization
Localization is driven by an `ITranslationProvider`. Use `ResxTranslationProvider` for a single `.resx` resource set, or `CompositeResxTranslationProvider` to combine several with a fallback chain (configured in `Setup` above). In XAML, resolve keys with the `Translate` markup extension:

```xml
<Label Text="{s:Translate WelcomeText}" />
```

#### Theming & Styles
The theme defined in `App.xaml` exposes its colors, spacing, and font sizes as dynamic resources, so controls react to runtime theme/language changes:

```xml
<Label
    FontSize="{DynamicResource Theme.FontSize.H1}"
    TextColor="{DynamicResource Theme.Color.Primary}" />
```

See the **StylesDemoPage** and **SpacingDemoPage**.

#### Effects
Attached effects add platform behavior without custom renderers — for example `TintImageEffect` to recolor an image, `LongPressEffect` for long-press commands, the line-color effects for input controls, and the safe-area padding effects (iOS):

```xml
<Image
    s:TintImageEffect.TintColor="{DynamicResource Theme.Color.Primary}"
    Source="camera_outline_black_192" />

<Label
    s:LongPressEffect.Command="{Binding LongPressCommand}"
    Text="Press and hold...">
    <Label.Effects>
        <s:LongPressEffect />
    </Label.Effects>
</Label>
```

#### Behaviors
Reusable behaviors include `EventToCommandBehavior` (turn any event into a command), `StatusBarBehavior` (set the status-bar color/style per page), and the `InputView*Behavior` family (max length, completed, unfocused handling):

```xml
<ContentPage.Behaviors>
    <s:EventToCommandBehavior
        Command="{Binding AppearingCommand}"
        EventName="Appearing" />
    <s:StatusBarBehavior
        StatusBarColor="{StaticResource Primary}"
        StatusBarStyle="Light" />
</ContentPage.Behaviors>
```

#### Converters & Extensions
The library bundles ~18 ready-to-use value converters for XAML bindings (e.g. `BoolInverter`, `BoolToColorConverter`, `IsNullToBoolConverter`, `NullableDateTimeToFormatConverter`, `ValidationErrorsToStringConverter`) under `Superdev.Maui/Converters`, plus a broad set of C# extension methods for `string`, `IEnumerable<T>`, `Color`, `TimeSpan`, `Type`, `Exception`, and more under `Superdev.Maui/Extensions`.

### Sample apps
The [TodoApp](Samples/TodoApp) is the place to start: a small, complete todo app which uses nothing but
the building blocks a real app needs - `UseSuperdevMaui`, page registrations, a start page, a tabbed main
page, a modal page, navigation parameters, a navigation guard, `BaseViewModel` and `IDialogService`.

The [SuperdevMauiDemoApp](Samples/SuperdevMauiDemoApp) is the reference: it demonstrates every feature in a runnable MAUI project, with dedicated demo pages for input controls, lists, theming and styles, validation, behaviors, effects, services, localization, and navigation.

### Contribution
Contributors welcome! If you find a bug or you want to propose a new feature, feel free to do so by opening a [new issue](https://github.com/superdevgmbh/Superdev.Maui/issues).

### License
This project is licensed under the [MIT License](LICENSE).
