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
Build up validation rules fluently with `ViewModelValidation` and expose them via your view model's `SetupValidation` override. Bind a control's `ValidationErrors` to `Validation.Errors[PropertyName]`:

```csharp
protected override ViewModelValidation SetupValidation()
{
    var validation = new ViewModelValidation();

    validation.AddValidationFor(nameof(this.UserName))
        .When(() => string.IsNullOrWhiteSpace(this.UserName))
        .Show(() => "Username must not be empty");

    return validation;
}
```

#### Navigation
`INavigationService` abstracts push/pop, modal, and root navigation. Register pages and view models with `RegisterForNavigation` and resolve them by type or by key:

```csharp
builder.Services.RegisterForNavigation<MainPage, MainViewModel>("MainPage");
builder.Services.RegisterForNavigation<EntryDemoPage, EntryDemoViewModel>();
```

```csharp
await this.navigationService.PushAsync<EntryDemoPage>();
```

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

### Sample app
The [SuperdevMauiDemoApp](Samples/SuperdevMauiDemoApp) demonstrates every feature in a runnable MAUI project, with dedicated demo pages for input controls, lists, theming and styles, validation, behaviors, effects, services, localization, and navigation.

### Contribution
Contributors welcome! If you find a bug or you want to propose a new feature, feel free to do so by opening a [new issue](https://github.com/superdevgmbh/Superdev.Maui/issues).

### License
This project is licensed under the [MIT License](LICENSE).
