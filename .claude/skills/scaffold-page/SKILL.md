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
             Title="{Page}">
</ContentPage>
```

The ContentPage element MUST have no child elements — leave the body completely empty.
Do NOT add Labels, StackLayouts, placeholder content, or any UI elements.
The developer will fill in the UI themselves.

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
namespace TheDeskWatch.MobileApp.Pages.{Feature}.ViewModels;

public sealed class {Page}ViewModel
{
}
```

The ViewModel body MUST be empty — no constructor, no properties, no fields.

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

Run `dotnet build TheDeskWatch.slnx` and report the result. The project enforces
`TreatWarningsAsErrors=true`, so a clean build means the scaffold is correct.
