# TodoApp

A minimal todo app built with [Superdev.Maui](../../README.md). It is meant as an introduction: it uses
only the building blocks a real app needs, and nothing else. For a tour of every feature of the library,
see the [SuperdevMauiDemoApp](../SuperdevMauiDemoApp) instead.

## What it shows

| Topic                                      | Where to look                                                                                                                                   |
|--------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------|
| Registering the library                    | [MauiProgram.cs](MauiProgram.cs) - `UseSuperdevMaui()`                                                                                          |
| Registering pages and view models          | [MauiProgram.cs](MauiProgram.cs) - `RegisterForNavigation<TPage, TViewModel>()`                                                                 |
| Naming pages in one place                  | [Pages.cs](Pages.cs)                                                                                                                            |
| Creating the start window from a URI       | [App.xaml.cs](App.xaml.cs) - `navigationService.CreateWindow("/AppStartPage")`                                                                  |
| Theme and colors                           | [App.xaml](App.xaml) - `ColorConfiguration` and `Theme`                                                                                         |
| A start page which replaces itself         | [AppStartViewModel.cs](ViewModels/AppStartViewModel.cs) - `INavigatedTo`                                                                        |
| Tabs with their own navigation stack       | [TabbedMainPage.xaml](Views/TabbedMainPage.xaml)                                                                                                |
| Selecting a tab programmatically           | [AboutViewModel.cs](ViewModels/AboutViewModel.cs) / [MainViewModel.cs](ViewModels/MainViewModel.cs) - `SelectTabAsync` by name and by view model type |
| Pushing a page onto another tab's stack    | [AboutViewModel.cs](ViewModels/AboutViewModel.cs) - `SelectTabAsync<MainViewModel>()` followed by `PushAsync`                                   |
| Loading data before a page is shown        | [MainViewModel.cs](ViewModels/MainViewModel.cs) - `BaseViewModel.InitializeAsync`                                                               |
| Reloading when a page becomes active again | [MainViewModel.cs](ViewModels/MainViewModel.cs) - `INavigatedTo`                                                                                |
| Pushing a page with a parameter            | [MainViewModel.cs](ViewModels/MainViewModel.cs) / [TodoDetailViewModel.cs](ViewModels/Todos/TodoDetailViewModel.cs) - `IInitialize<TParameter>` |
| A modal page inside a navigation page      | [MainViewModel.cs](ViewModels/MainViewModel.cs) - `PushModalAsync(..., wrapInNavigationPage: true)`                                             |
| Vetoing a navigation                       | [AddTodoViewModel.cs](ViewModels/Todos/AddTodoViewModel.cs) - `IConfirmNavigation`                                                              |
| Save, discard or stay when leaving a dirty page | [TodoDetailViewModel.cs](ViewModels/Todos/TodoDetailViewModel.cs) - `IConfirmNavigation` with an action sheet                              |
| Alerts and confirmations                   | `IDialogService` in the view models                                                                                                             |
| Injectable, testable services              | [TodoService.cs](Services/TodoService.cs) - `IPreferences`, `IDateTime`                                                                         |

## Structure

```
App.xaml(.cs)             Theme and the start window
Pages.cs                  The navigation names of all pages
MauiProgram.cs            Library setup, services, page registrations
Models/                   TodoItem
Services/                 ITodoService / TodoService (stored in the preferences)
ViewModels/               AppStartViewModel, MainViewModel, AboutViewModel
ViewModels/Todos/         AddTodoViewModel, TodoDetailViewModel, TodoItemViewModel
Views/                    AppStartPage, TabbedMainPage, MainPage, AboutPage
Views/Todos/              AddTodoPage, TodoDetailPage, TodoItemTemplate
```

The page flow is:

```
AppStartPage                                    the start window, replaces itself after the startup work
└── TabbedMainPage
    ├── NavigationPage > MainPage               the todo list
    │   └── TodoDetailPage                      pushed with a navigation parameter
    └── NavigationPage > AboutPage
                                                AddTodoPage is pushed modally on top of everything
```

## Running it

```bash
dotnet build Samples/TodoApp/TodoApp.csproj -f net10.0-ios -p:Platform=Simulator
```

```bash
dotnet build Samples/TodoApp/TodoApp.csproj -f net10.0-android
```

## Good to know

Three things about this app are worth pointing out, because they are easy to get wrong:

- **No view model base class of your own.** `Superdev.Maui.Mvvm.BaseViewModel` already implements
  `IInitialize`, and every other lifecycle callback is an interface a view model opts into. The view
  models here derive from `BaseViewModel` directly and implement only what they actually need.
- **The start navigation runs in `INavigatedTo`, not in `InitializeAsync`.** Navigating from
  `IInitialize`, `INavigatedFrom` or a navigation guard throws, because those callbacks run while the
  navigation is still in progress. `AppStartViewModel` therefore navigates from `INavigatedTo`, which
  runs once the navigation to the start page completed - no `async void` needed.
- **Tabs are selected through the navigation service, not through `CurrentPage`.** `SelectTabAsync` takes
  the navigation name or the view model type of the page a tab hosts, delivers the lifecycle callbacks to
  the tab child the same way a tap on the tab bar does, and initializes a tab which was never shown before
  it becomes visible. Afterwards the ordinary navigation calls apply to the navigation stack of that tab,
  which is how `AboutViewModel` opens a todo on the todos tab.
- **A navigation guard can do more than veto.** `TodoDetailViewModel.CanNavigateAsync` offers to save,
  discard or stay when the todo has unsaved changes. Choosing to save stores the todo and then lets the
  navigation proceed; a failed save keeps the page. The guard runs whichever way the page is about to be
  left - the back button, the hardware back button, a pop from code or a deep link - because all of those go
  through the navigation service.
- **Navigation pages declared in XAML are `s:CustomNavigationPage`.** The navigation service registers
  `CustomNavigationPage` under the name `NavigationPage`, so a navigation URI gets it automatically - but a
  tab declared in XAML gets whatever the XAML says. `CustomNavigationPage` routes the Android hardware back
  button through the navigation service, which is what makes the guard of the todo detail page work there;
  a plain `NavigationPage` pops without asking.
- **Tabs declared in XAML get their view model assigned by hand.** The navigation service creates the
  pages it navigates to and gives them the view model from their registration. The tab children of
  `TabbedMainPage` are created by the XAML parser instead, so its code-behind resolves `MainViewModel`
  and `AboutViewModel` from the page's service scope and assigns them.
