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

## Step 0 — Ensure the MVVM toolkit is referenced

The scaffolded ViewModel uses [`CommunityToolkit.Mvvm`](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
(`ObservableObject` + `[RelayCommand]`). Check whether
`src/TheDeskWatch.MobileApp/TheDeskWatch.MobileApp.csproj` already has a
`<PackageReference Include="CommunityToolkit.Mvvm" .../>`.

If it does **not**, the `.csproj` is guardrailed (see CLAUDE.md): stop and ask the user to
approve adding the package before continuing. Once approved, add:

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
```

(use the latest stable version). Do not proceed with the scaffold until the package is present,
or the build in Step 4 will fail.

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

- The page `Title` MUST bind to the ViewModel's `Title` property (`Title="{Binding Title}"`),
  never a hard-coded string.
- The only child is the single `Button` above, bound to the ViewModel's generated
  `DoNothingCommand`. Do NOT add any other UI (Labels, StackLayouts, placeholder content) —
  the developer fills in the rest themselves.
- Keep the button free of inline style literals (no hard-coded colors, sizes, or spacing),
  per the UI Styling Rules in CLAUDE.md.

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

The ViewModel:

- Is a `sealed partial` class deriving from `ObservableObject` (required for the
  `[RelayCommand]` source generator).
- MUST declare a constructor that assigns the `Title` property — the title shown by the page.
  Default it to the `{Page}` name unless the user specifies a different title.
- Exposes a `Title` property bound by the page's `Title="{Binding Title}"`.
- Declares a single `[RelayCommand]`-attributed `DoNothing` method with an empty body. The
  generator produces the `DoNothingCommand` property the button binds to. It intentionally
  does nothing — the developer fills in the behavior later.

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
