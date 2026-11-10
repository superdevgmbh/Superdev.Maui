global using System.Collections.ObjectModel;
global using System.Windows.Input;

global using CommunityToolkit.Mvvm.Input;

global using Microsoft.Extensions.Logging;

global using Superdev.Maui.Mvvm;
global using Superdev.Maui.Navigation;
global using Superdev.Maui.Services;

global using TodoApp.Models;
global using TodoApp.Services;

// Superdev.Maui extends a few abstractions of Microsoft.Maui, so these aliases pick the Superdev.Maui
// ones, which are the types registered by UseSuperdevMaui.
global using IDeviceInfo = Superdev.Maui.Services.IDeviceInfo;
global using IPreferences = Superdev.Maui.Services.IPreferences;
