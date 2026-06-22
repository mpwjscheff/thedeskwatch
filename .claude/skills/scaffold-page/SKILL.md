---
name: scaffold-page
description: >
  Scaffolds a new MAUI feature page for the TheDeskWatch app: creates the .xaml page,
  .xaml.cs code-behind, and ViewModel, then wires up DI registration in MauiProgram.cs
  and route registration in AppShell.xaml.cs. Use this skill whenever the user asks to
  create a new page, add a screen, scaffold a view, or set up a new feature page in the
  project. Always use this skill for any page creation task — do not write these files
  manually without it. Triggers on create/add/scaffold/new X page/screen/view/feature.
---

# Scaffold MAUI Page

Generates the complete presentation-layer scaffold for a new feature page in TheDeskWatch,
following the project's feature-first MVVM conventions from CLAUDE.md.

## Extract inputs from the user's request

- **Feature** — PascalCase feature name (e.g. `Home`, `Settings`, `Timer`).
- **Page** — PascalCase page name (e.g. `TimerDetail`, `Settings`). Defaults to the Feature name when not specified.

If either is ambiguous, ask before proceeding.

## Step 1 — Create the three new files

All paths are relative to the repository root. Create parent directories as needed.

### Page XAML — `src/TheDeskWatch.MobileApp/Pages/{Feature}/Pages/{Page}Page.xaml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:TheDeskWatch.MobileApp.Pages.{Feature}.ViewModels"
             x:Class="TheDeskWatch.MobileApp.Pages.{Feature}.Pages.{Page}Page"
             x:DataType="vm:{Page}ViewModel"
             Title="{Binding Title}">
    <Button Text="Do Nothing"
            Command="{Binding DoNothingCommand}" />
</ContentPage>
```

- `Title` binds to the ViewModel's `Title` — never a hard-coded string.
- The single `Button` is the *only* child — don't add other UI (Labels, layouts, placeholder
  content); the developer fills in the rest.

### Page code-behind — `src/TheDeskWatch.MobileApp/Pages/{Feature}/Pages/{Page}Page.xaml.cs`

```csharp
namespace TheDeskWatch.MobileApp.Pages.{Feature}.Pages;

public partial class {Page}Page : ContentPage
{
    public {Page}Page({Page}ViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

### ViewModel — `src/TheDeskWatch.MobileApp/Pages/{Feature}/ViewModels/{Page}ViewModel.cs`

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TheDeskWatch.MobileApp.Pages.{Feature}.ViewModels;

public sealed partial class {Page}ViewModel : ObservableObject
{
    public {Page}ViewModel()
    {
        Title = "{Page}";
    }

    public string Title { get; }

    [RelayCommand]
    private void DoNothing()
    {
    }
}
```

Two things to get right beyond what the template shows:

- The constructor assigns `Title` — default it to the `{Page}` name unless the user specifies a different title.
- The `DoNothing` `[RelayCommand]` is an intentional empty placeholder; the developer fills in the behavior later.

## Step 2 — Register in DI (`src/TheDeskWatch.MobileApp/MauiProgram.cs`)

Read the file. Add the two `using` directives at the top (after existing usings, if not already present):

```csharp
using TheDeskWatch.MobileApp.Pages.{Feature}.Pages;
using TheDeskWatch.MobileApp.Pages.{Feature}.ViewModels;
```

Add the two `AddTransient` calls just before `return builder.Build()`:

```csharp
builder.Services.AddTransient<{Page}Page>();
builder.Services.AddTransient<{Page}ViewModel>();
```

## Step 3 — Register the route (`src/TheDeskWatch.MobileApp/AppShell.xaml.cs`)

Read the file. Add the `using` directive at the top (if not already present):

```csharp
using TheDeskWatch.MobileApp.Pages.{Feature}.Pages;
```

Add the `Routing.RegisterRoute` call inside the `AppShell` constructor, after `InitializeComponent()`:

```csharp
Routing.RegisterRoute(nameof({Page}Page), typeof({Page}Page));
```

## Step 4 — Verify

Run `dotnet build TheDeskWatch.slnx` and report the result. A clean build means the scaffold is correct.
