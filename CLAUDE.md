# CLAUDE.md

## Project Overview

TheDeskWatch is a .NET 10 MAUI cross-platform app targeting Android and iOS. The solution uses the modern `.slnx` format.

## Commit Message Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/): `<type>(<scope>): <description>.`

- **One line only** (no body/footer), **ends with a period**, **no AI attribution** (no `Co-Authored-By`, "Generated with", or tool names).
- **`<type>`**: `feat`, `fix`, `refactor`, `test`, `chore`, `docs`, `style`, `perf`, or `ci`.
- **`<scope>`**: the feature or layer affected (e.g. `home`, `persistence`, `auth`); omit when global.
- **`<description>`**: imperative, lowercase, concise.

Example: `feat(home): add clock widget to home page.`

## Agent Workflow

> **Never work directly on `main`.** Every task — bug fix, feature, spike — gets its own worktree + branch.

```powershell
git worktree add ..\TheDeskWatch-<branch-name> -b <branch-name>  # create (from repo root)
# ...do all edits in the new worktree path, then open a PR from that branch...
git worktree remove ..\TheDeskWatch-<branch-name>; git branch -d <branch-name>  # clean up after merge
```

### Specialized subagents

Three project subagents live in `.claude/agents/`, each scoped to one layer. Delegate matching work to them; they enforce that layer's conventions and report back the files they changed plus any cross-layer contract the next agent must honour.

- **`ui-xaml`** — XAML views, code-behind, `Resources/Styles`, value converters, and data-binding (`*.xaml` + `Resources/Styles`). Compiled bindings (`x:DataType`), tokens-only styling, no logic in code-behind.
- **`app-logic`** — ViewModels, services, models, DI registration, and Shell navigation (`ViewModels/`, `Services/`, `Models/`, `MauiProgram.cs`). MVVM source generators, services behind interfaces, native calls kept behind an interface.
- **`platform-native`** — everything under `Platforms/Android` and `Platforms/iOS`: permissions, `AndroidManifest.xml`, `Info.plist`, handlers, native interop, and platform service implementations of the interfaces `app-logic` defines.

## First-time setup

After cloning, point git at the project hooks so every commit first compiles the solution and runs the unit tests:

```powershell
git config core.hooksPath .githooks
```

## Build & Run Commands

```powershell
dotnet build TheDeskWatch.slnx                                      # build solution
dotnet build src/TheDeskWatch.MobileApp -f net10.0-android -c Debug # build for Android
dotnet build src/TheDeskWatch.MobileApp -f net10.0-ios -c Debug     # build for iOS
dotnet test TheDeskWatch.slnx                                       # run all tests (unit + architecture)
dotnet test tests/TheDeskWatch.Application.Tests                    # run Application unit tests only
dotnet test tests/TheDeskWatch.Architecture.Tests                   # run Architecture (layering + convention) tests only
```

## Static Analysis Guardrails

`Directory.Build.props` sets `TreatWarningsAsErrors=true` and `AnalysisLevel=latest-recommended` across every project, so every Roslyn diagnostic fails the build. Prefer fixing diagnostics at the root over suppressing them (`#pragma warning disable`, `[SuppressMessage]`, `<NoWarn>`); the one standing suppression is `CA1707` in `tests/Directory.Build.props` for underscore-named test methods.

## Architecture

Three-tier architecture, reflected in the solution folders (`/10. PRESENTATION/`, `/20. APPLICATION/`, `/30. PERSISTENCE/`, `/50. TESTS/`):

- **`MobileApp/`** — MAUI presentation layer; the only project that may reference the MAUI SDK. Entry point `MauiProgram.cs` (DI setup), navigation shell `AppShell.xaml`. Platform code lives in `Platforms/Android/` and `Platforms/iOS/`; cross-platform code at the project root. XAML Source Generation is enabled.
- **`MobileApp.Contracts/`** — MAUI-agnostic interfaces for platform capabilities (e.g. `IFileService`, `IPermissionsService`). Plain .NET library, no MAUI reference. The only project allowed to define such interfaces; `Application` and `Persistence` reference it to use a platform capability.
- **`Application/`** — Business logic and use cases, organised feature-first under `Features/`.
- **`Domain/`** — Domain models as stored by `Persistence`.
- **`Persistence/`** — Data access, repositories.
- **`Application.Tests/`** — Unit tests for the Application layer.
- **`Architecture.Tests/`** — `NetArchTest` rules that fail the build if a layer dependency or an Application naming/visibility convention is broken (see [Testing](#testing)).

### Adding a platform capability

1. Define the interface in `MobileApp.Contracts` (e.g. `IFileService`).
2. Implement it in `MobileApp/Services/` (e.g. `MauiFileService.cs`).
3. Register the implementation in `MauiProgram.cs`.
4. Layers needing it (`Application`, `Persistence`) depend on the interface — never on the MAUI implementation.

## Presentation (`MobileApp`)

Feature-first MVVM under a top-level `Pages/` folder. Each feature gets its own subfolder — never add page/viewmodel files to the `Pages/` root.

```
Pages/Home/
├── Views/        # Reusable self-contained UI fragments (ContentView, DataTemplate)
├── Pages/        # Full pages: one .xaml + one .xaml.cs each
└── ViewModels/   # One ViewModel per page
```

- **ViewModels** depend only on `Application` (use cases, services, DTOs); never on `Persistence`, `Domain`, or any repository type.
- **Pages** code-behind is thin: set `BindingContext` to the injected ViewModel; nothing else beyond UI lifecycle (e.g. `OnAppearing`).
- **DI**: every page and ViewModel is registered in `MauiProgram.cs` (e.g. `AddTransient<HomePage>()`) and resolved by constructor injection — never created manually.

### MVVM (`CommunityToolkit.Mvvm`)

ViewModels use [`CommunityToolkit.Mvvm`](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) — the standard for the presentation layer. Never hand-roll `INotifyPropertyChanged`, `ICommand`, or `Command`.

- **Naming**: a ViewModel backing a `Page` uses the `PageViewModel` postfix (e.g. `HomePageViewModel`); a ViewModel backing a reusable `View` (`ContentView`, `DataTemplate`) uses the plain `ViewModel` postfix (e.g. `ClockWidgetViewModel`).
- Derive every ViewModel from `ObservableObject` and declare it `partial` (the toolkit's source generators require it).
- Expose bindable state with `[ObservableProperty]` on a `private` backing field.
- Define commands with `[RelayCommand]` on a method; bind to the generated `{Method}Command` property.
- A ViewModel defines a constructor that sets its page `Title`; the page binds `Title="{Binding Title}"` rather than hard-coding a string.

```csharp
public sealed partial class HomePageViewModel : ObservableObject
{
    public HomePageViewModel() => Title = "Home";

    public string Title { get; }

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    private void Refresh() { }
}
```

### Services vs. Helpers

- **`Services/`** — MAUI implementations of `MobileApp.Contracts` interfaces. One class per interface, `Maui` prefix (e.g. `MauiFileService : IFileService`). Nothing else.
- **`Helpers/`** — thin, (near-)stateless abstractions over internal MAUI concerns so nothing calls a framework API directly. E.g. a navigation service wrapping `Shell.Current.GoToAsync`, or an `IDialogService` as the single caller of `DisplayAlert`. Pages/ViewModels call these services, never the framework API.

## Application layer

Uses [LiteBus](https://github.com/litenova/LiteBus) as mediator (`LiteBus.Commands.Abstractions`, `LiteBus.Queries.Abstractions`). Inject `ICommandMediator` / `IQueryMediator` into ViewModels. Registered in `MauiProgram.cs`:

```csharp
builder.Services.AddLiteBus(config =>
    config.AddCommandModule()
          .AddQueryModule()
          .RegisterFromAssembly(typeof(MauiProgram).Assembly)
          .RegisterFromAssembly(typeof(SomeApplicationType).Assembly));
```

Feature-first under `Features/{Feature}/` (`Home`, `Settings`, …), each with `Commands/`, `Queries/`, and optional `Services/`. Commands and queries always return `OneOf<T, ApiError>` (success first) — handlers never propagate exceptions to the caller. Scaffold them with the `scaffold-command` and `scaffold-query` skills, which emit architecture-compliant boilerplate (templates + full rules live there); `Architecture.Tests` then enforces the naming/visibility conventions below.

### Commands

Command + handler in one file under `Features/{Feature}/Commands/`. The command is `public sealed`; the handler is `internal sealed` with a primary constructor, catching exceptions internally and mapping to `ApiError`. `CancellationToken` is last and defaults to `default`.

### Queries

Query, response DTO, and handler in one file under `Features/{Feature}/Queries/`. The query is `public` (non-sealed) using interface-shorthand syntax; the response is a `public sealed record` (nested models inside it); the handler is `public sealed` with a primary constructor. `CancellationToken cancellationToken = new()` is last. Private helpers swallow their own errors and return a safe default, so `HandleAsync` needs no top-level try/catch.

### Feature services

Logic shared across commands/queries goes in `Features/{Feature}/Services/`: define an interface, inject the implementation via DI — never call the concrete class directly.

## Testing

- `Application.Tests` tests the Application layer exclusively — every command, query, and service must have corresponding tests.
- Mirror the source folder structure: `Application/Features/Home/Commands/CreateTask/…` → `Application.Tests/Features/Home/Commands/CreateTask/…Tests.cs`.
- Never cross layer boundaries — stub/mock `Persistence` and platform dependencies; never reference `TheDeskWatch.Persistence` or `TheDeskWatch.MobileApp`.
- `Architecture.Tests` machine-enforces the layering rules (Architecture) and the Application Commands/Queries naming + visibility conventions (Application layer) — so those rules fail the build, not just review. It cannot reference `MobileApp` (MAUI targets `net10.0-android/ios`), so ViewModel-layer rules stay review-only. When you add or change an architectural rule here, add or update its test there.

## Key Conventions

- Nullable reference types are enabled everywhere — annotate accordingly.
- `<ImplicitUsings>enable</ImplicitUsings>` is on; common namespaces need no explicit `using`.
- Place every `using` directive at the top of the file, outside (above) the namespace declaration — never inside the namespace block.
- Prefer abstraction over direct framework calls: if a concern (alerts, navigation, permissions, …) is used in more than one place, introduce a service and inject it — Contracts implementations in `Services/`, internal MAUI abstractions in `Helpers/`, infrastructure concerns the same way inside `Application`.

## UI Styling Rules

Every design token — colors, font sizes, spacing, corner radii — must live in the resource files and be referenced by key (`{StaticResource KeyName}`). Never inline a literal color, size, or spacing value in page/view XAML.

- **Single source of truth**: colors in `Resources/Styles/Colors.xaml`; sizes, margins, corner radii, font sizes, and composite styles in `Resources/Styles/Styles.xaml`.
- **Light & dark mode**: every color needs both variants via `AppThemeBinding` — never add a color that works in only one mode.
- **Naming**: semantic, purpose-based keys (`SurfaceBackground`, `PrimaryText`, `AccentColor`), not appearance-based.
- **Component library**: prefer a built-in MAUI control; if none fits, use the [.NET MAUI Community Toolkit](https://github.com/CommunityToolkit/Maui) before any other library or a custom control.
