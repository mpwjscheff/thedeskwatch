# Welcome Screen — Implementation Plan

## Overview

A full-screen welcome page shown every time the app launches. It displays the TheDeskWatch logo and a single "Get started" button. Tapping the button navigates the user into the main app (AppShell with the tab bar). This page lives entirely in the presentation layer — it has no business logic, no data to load, and no persistence.

## Acceptance criteria

- [ ] The welcome page is the first thing the user sees on every app launch (before the tab bar)
- [ ] The page shows the TheDeskWatch logo centred on screen
- [ ] A "Get started" button appears below the logo
- [ ] Tapping "Get started" replaces the welcome page with the AppShell (tab bar becomes visible, no back navigation possible)
- [ ] The page background uses a warm cream pastel colour that complements the logo
- [ ] The page looks correct in both light and dark mode

## Architecture decisions

- **No persistence**: the page shows every launch, so there is no "has seen welcome" flag to store
- **No Application layer**: no commands, queries, or feature services are needed — the feature is purely presentational
- **No Contracts**: no platform capabilities (camera, file system, etc.) are required
- **Navigation strategy**: `App.xaml.cs` sets `MainPage` to `WelcomePage` on startup; the ViewModel's `GetStarted` command switches `Application.Current!.MainPage` to a freshly resolved `AppShell`, preventing any back-navigation to the welcome page
- **Logo image**: the TheDeskWatch logo PNG is stored in `Resources/Images/` so MAUI's image pipeline handles multi-density scaling automatically

## Layers

### Presentation (`MobileApp/`)

**New files to create:**

- `Pages/Welcome/Pages/WelcomePage.xaml` + `WelcomePage.xaml.cs`
- `Pages/Welcome/ViewModels/WelcomePageViewModel.cs`

**Image asset:**

- Add the logo PNG as `Resources/Images/logo.png` (MAUI will generate the platform-specific sizes from this single source file)

**ViewModel shape:**

- Constructor sets `Title = "Welcome"`
- `[RelayCommand]` method `GetStarted` — resolves `AppShell` from the DI container and assigns it to `Application.Current!.MainPage`; the ViewModel receives `IServiceProvider` in its constructor to perform this resolution

**Page layout (WelcomePage.xaml):**

- Root `ContentPage` background bound to the `WelcomeBackground` colour token
- Vertically centred `VerticalStackLayout` containing:
  - `Image` source pointing to `logo.png`, with a fixed width (e.g. 260 dp) and `Aspect = AspectFit`
  - A `Button` with text `"Get started"` bound to `GetStartedCommand`, styled using existing app button style tokens

**DI + navigation:**

- `MauiProgram.cs`: register `WelcomePage` and `WelcomePageViewModel` with `AddTransient<>()`
- `App.xaml.cs`: resolve `WelcomePage` from the service provider and assign it to `MainPage` in the constructor (replacing the current default `AppShell` assignment)
- No Shell route needed — the welcome page is never navigated to via `GoToAsync`

**Styling:**

- Add a `WelcomeBackground` colour key to `Resources/Styles/Colors.xaml` with:
  - Light value: warm cream — approximately `#F5EFE0` (matches the logo's internal background)
  - Dark value: a desaturated warm dark tone — approximately `#2A2520` — so the logo remains legible without losing the warm feel
- Both variants must use `AppThemeBinding`

### Application (`Application/`)

None. This feature has no commands, queries, or feature services.

### Domain (`Domain/`)

None.

### Persistence (`Persistence/`)

None.

### Contracts (`MobileApp.Contracts/`)

None.

## Implementation order

1. **Image asset** — drop the logo PNG into `Resources/Images/`; verify it builds without errors
2. **Colour tokens** — add `WelcomeBackground` (light + dark) to `Colors.xaml`
3. **ViewModel** — create `WelcomePageViewModel` with the `GetStarted` command
4. **Page XAML + code-behind** — create `WelcomePage` using the new colour token and logo image
5. **DI wiring** — register page + ViewModel in `MauiProgram.cs`; update `App.xaml.cs` to set `MainPage` to `WelcomePage`

## Open questions

- The exact shade of the cream pastel and its dark-mode counterpart should be verified visually once the page renders on device — tweak the hex values in `Colors.xaml` if the contrast feels off.
- If `App.xaml.cs` currently resolves `AppShell` via the service provider, confirm that pattern is already established so the same approach works for `WelcomePage`; if `AppShell` is constructed inline (`new AppShell()`), adjust accordingly.
