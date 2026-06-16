---
name: add-component-inside-page
description: >
  Updates an existing MAUI page (.xaml, code-behind, and ViewModel) in TheDeskWatch to insert
  a new UI component or view. Decides automatically whether to create a reusable ContentView
  under the feature's Views/ folder or insert XAML inline in the page, based on complexity and
  reusability. Scaffolds ViewModel TODOs for any needed bindings or commands. Finishes with a
  dotnet build to verify correctness.

  Use this skill whenever the user wants to add, insert, or show something new on an existing
  page — "add a clock widget to the home page", "show a task list on the settings page",
  "I want a button that does X on the Y page", "put a [component] on [page]", "add a [thing]
  to the [screen]". Trigger even if the user doesn't say "component" or "view" — any request
  to make something appear on an existing screen counts.
---

# Add UI Component to Existing Page

Updates an existing MAUI page in TheDeskWatch to include a new UI component, following
the project's feature-first MVVM conventions.

## Step 1 — Identify the target

Extract from the user's request:

- **Feature** — PascalCase folder name (e.g. `Home`, `Settings`, `Timer`).
- **Page** — PascalCase page name without the `Page` suffix (e.g. `Home`, `Settings`). Defaults to the Feature name when not specified.
- **Component** — what the user wants to see (e.g. "a clock widget", "a list of tasks", "a save button").

If Feature or Page is ambiguous, scan `src/TheDeskWatch.MobileApp/Pages/` to find the right match.
Confirm with the user only if genuinely unclear after scanning.

Resolve the three file paths:
- Page XAML: `src/TheDeskWatch.MobileApp/Pages/{Feature}/Pages/{Page}Page.xaml`
- Code-behind: `src/TheDeskWatch.MobileApp/Pages/{Feature}/Pages/{Page}Page.xaml.cs`
- ViewModel: `src/TheDeskWatch.MobileApp/Pages/{Feature}/ViewModels/{Page}ViewModel.cs`

Read all three before making changes.

## Step 2 — Choose: ContentView or inline XAML

**Create a ContentView** (`Pages/{Feature}/Views/{ComponentName}.xaml`) when the component:
- Has a self-contained visual identity (a widget, a card, a panel, a form section)
- Groups multiple controls that belong together as a unit
- Has a name the user gave it (e.g. "a clock widget", "a task card", "a stats panel")
- Could plausibly be reused on another page

**Insert XAML inline** directly in the page when the component:
- Is a single control or a trivial grouping (a button, a label, a text entry, a toggle)
- Is clearly a one-off addition that belongs only to this page
- The user describes it by what it does, not what it is ("a button that saves", "a label showing the title")

When in doubt, prefer inline — it's simpler and easier to promote to a ContentView later.

## Step 3a — If ContentView: create the two new files

**ContentView XAML** — `src/TheDeskWatch.MobileApp/Pages/{Feature}/Views/{ComponentName}.xaml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TheDeskWatch.MobileApp.Pages.{Feature}.Views.{ComponentName}">
    <!-- component XAML here — see Step 4 for styling rules -->
</ContentView>
```

Fill in the component's XAML controls inside the ContentView. The BindingContext is
inherited from the parent page, so bindings to the page's ViewModel work out of the box.

**ContentView code-behind** — `src/TheDeskWatch.MobileApp/Pages/{Feature}/Views/{ComponentName}.xaml.cs`

```csharp
namespace TheDeskWatch.MobileApp.Pages.{Feature}.Views;

public partial class {ComponentName} : ContentView
{
    public {ComponentName}()
    {
        InitializeComponent();
    }
}
```

## Step 3b — Update the page XAML

Read the current page XAML and insert the new component at the logical position described
by the user, or at the bottom of the existing layout when unspecified.

**If ContentView:** add the `views` XML namespace to the `<ContentPage>` opening tag (after
existing `xmlns:` declarations, if not already present) and insert the element:

```xml
xmlns:views="clr-namespace:TheDeskWatch.MobileApp.Pages.{Feature}.Views"
...
<views:{ComponentName} />
```

**If inline:** insert the XAML controls directly at the appropriate spot in the layout.

**If the page body is empty** (no root layout yet — common right after `scaffold-page`),
wrap the new content in a `<VerticalStackLayout>` as the root child of `<ContentPage>`.

The code-behind (`{Page}Page.xaml.cs`) almost never needs changes when adding a component —
only touch it if the component requires a specific lifecycle hook (e.g. `OnAppearing`).

## Step 4 — XAML styling rules

Every design token must come from the project's resource files — never hardcode a value.

- **Colors**: use `{AppThemeBinding Light={StaticResource X}, Dark={StaticResource Y}}` with
  keys from `Resources/Styles/Colors.xaml` (e.g. `Primary`, `Gray500`, `OffBlack`, `White`).
  Never use a bare color literal or a single-mode `{StaticResource}` on a color property.
- **Named styles**: use `{StaticResource KeyName}` for keys already defined in
  `Resources/Styles/Styles.xaml` (e.g. `Style="{StaticResource Headline}"`).
- **Font sizes, spacing, corner radii**: if a `Style` in `Styles.xaml` already covers the
  control type (Button, Label, Entry, etc.), rely on it implicitly — don't repeat the same
  setters inline. Add explicit values only when deviating from the implicit style.
- **New tokens**: if the component needs a value not in the resource files, add it there
  with a semantic key (`{StaticResource CardCornerRadius}`), not inline.
- **Controls**: prefer built-in MAUI controls; if none fits, use .NET MAUI Community Toolkit
  before any other library.

## Step 5 — Scaffold ViewModel TODOs

Read the existing ViewModel. If the component will need data bindings, commands, or injected
services, add a comment block listing what's needed — don't implement anything:

```csharp
// TODO: add for {ComponentName}
// - property: public string Title { get; set; }
// - command: public ICommand SaveCommand { get; }
// - inject: ICommandMediator (for dispatching a command on tap)
```

Leave the ViewModel class body otherwise unchanged. The developer will fill these in.

## Step 6 — Build and report

Run `dotnet build TheDeskWatch.slnx` and report the result. The project enforces
`TreatWarningsAsErrors=true`, so a clean build confirms the scaffold is correct.
If the build fails, read the error, fix it, and rebuild before reporting.
