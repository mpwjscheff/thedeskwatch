# Colleagues List Management — Implementation Plan

## Overview

Introduces a dedicated Colleagues tab showing a simple list of all colleagues (name only), with the ability to add a new colleague and edit an existing one via a bottom sheet containing a name field and a color swatch picker. The current "Colleagues" tab (stand-up bubble tracker) is renamed to "Log" and gets a new icon. The new Colleagues list becomes the primary entry point for managing colleague data.

## Acceptance criteria

- [ ] The tab bar has two tabs: "Colleagues" (list) and "Log" (stand-up bubbles, renamed)
- [ ] The Colleagues tab shows all colleagues as a plain list of full names
- [ ] Tapping "+" in the navigation bar opens the add bottom sheet with an empty name field and no color pre-selected
- [ ] Tapping a colleague in the list opens the edit bottom sheet pre-populated with their name and current color
- [ ] The bottom sheet contains a text field for full name and a row of predefined color swatches
- [ ] Saving an add persists a new colleague to SQLite and the list refreshes
- [ ] Saving an edit persists the updated name and color to SQLite and the list refreshes
- [ ] Saving is disabled (or shows validation feedback) when the name field is empty
- [ ] The bottom sheet dismisses on successful save

## Architecture decisions

- **Color selection**: predefined set of 10–12 color swatches displayed as tappable circles inside the bottom sheet. No free-form color picker — MAUI Community Toolkit does not ship one, and swatches are more appropriate in a constrained bottom sheet layout.
- **Bottom sheet**: `CommunityToolkit.Maui.Views.BottomSheet` from the MAUI Community Toolkit, which is already a project dependency.
- **Id propagation**: `ColleagueRecord` (Contracts) and `ColleagueDto` (Application query response) both need an `Id` field added so the edit path can target the correct row.
- **Existing page rename**: the current `ColleaguesPage` (stand-up bubbles) is renamed to `LogPage`; its tab label becomes "Log" and gets a clock/history icon. File and class names are updated accordingly across XAML, code-behind, ViewModel, and DI registrations.
- **Storage**: SQLite via the existing EF Core `DeskWatchDbContext` — no new tables needed, just new repository methods.

## Layers

### Presentation (`MobileApp/`)

**Files to rename (existing stand-up page → Log):**
- `Pages/Colleagues/Pages/ColleaguesPage.xaml` → `Pages/Log/Pages/LogPage.xaml` + `LogPage.xaml.cs`
- `Pages/Colleagues/ViewModels/ColleaguesPageViewModel.cs` → `Pages/Log/ViewModels/LogPageViewModel.cs`
- `Pages/Colleagues/ViewModels/ColleagueViewModel.cs` → move to `Pages/Log/ViewModels/ColleagueViewModel.cs` (or keep shared)
- `Pages/Colleagues/Messages/StandUpToastMessage.cs` → move into `Pages/Log/Messages/`

**New files to create (Colleagues list page):**
- `Pages/Colleagues/Pages/ColleaguesPage.xaml` + `ColleaguesPage.xaml.cs`
- `Pages/Colleagues/ViewModels/ColleaguesPageViewModel.cs`
- `Pages/Colleagues/Views/ColleagueFormBottomSheet.xaml` + `ColleagueFormBottomSheet.xaml.cs`
- `Pages/Colleagues/ViewModels/ColleagueFormViewModel.cs`

**ColleaguesPageViewModel shape:**
- `[ObservableProperty]` fields: `ObservableCollection<ColleagueListItemViewModel> Colleagues`
- `[RelayCommand]` methods: `AddColleague()` (opens bottom sheet in add mode), `EditColleague(ColleagueListItemViewModel item)` (opens bottom sheet in edit mode)
- `LoadAsync()` called from `OnAppearing` in code-behind
- Constructor sets `Title = "Colleagues"` and injects `IQueryMediator` + `ICommandMediator`

**ColleagueListItemViewModel shape:**
- Properties: `int Id`, `string Name`, `Color BubbleColor` (converted from hex at load time)
- Simple data-holder, no commands

**ColleagueFormViewModel shape:**
- `[ObservableProperty]` fields: `string Name`, `ColleagueColorSwatch? SelectedSwatch`
- `IReadOnlyList<ColleagueColorSwatch> AvailableSwatches` — initialized in constructor from a hardcoded palette of 10–12 colors (each swatch has a `Color` and its hex string)
- `bool IsEditMode` — set to true and `Id` stored when opened for editing
- `[RelayCommand]` method: `Save()` — dispatches `AddColleagueCommand` or `UpdateColleagueCommand` based on mode; on success raises a `Saved` event the page observes to dismiss the sheet and trigger a list reload
- `SaveCommand.CanExecute` returns false when `Name` is null or whitespace

**ColleagueFormBottomSheet:**
- `ContentTemplate` binding context is `ColleagueFormViewModel` (injected)
- Contains a text field bound to `Name` and a horizontal `CollectionView` (or `FlexLayout`) of color swatches bound to `AvailableSwatches`, with `SelectedSwatch` driving a selection indicator
- A "Save" button bound to `SaveCommand`

**DI + navigation (`MauiProgram.cs`):**
- Remove registrations for old `ColleaguesPage` / `ColleaguesPageViewModel`
- Add `AddTransient<LogPage>()`, `AddTransient<LogPageViewModel>()`
- Add `AddTransient<ColleaguesPage>()`, `AddTransient<ColleaguesPageViewModel>()`
- Add `AddTransient<ColleagueFormBottomSheet>()`, `AddTransient<ColleagueFormViewModel>()`

**AppShell.xaml:**
- First tab: "Colleagues" → routes to new `ColleaguesPage`; icon `&#xf0c0;` (FontAwesome6Solid `users`) — keeps the people-group glyph which suits a contacts list
- Second tab: "Log" → routes to renamed `LogPage`; icon `&#xf017;` (FontAwesome6Solid `clock`) — suits the stand-up time-tracking concept

### Application (`Application/`)

**Queries** (`Features/Colleagues/Queries/`):

| File | Change | Parameters | Response DTO | What it returns |
|---|---|---|---|---|
| `GetColleagues.cs` | Update existing | none | `ColleagueDto(int Id, string Name, string HexColor)` | All colleagues ordered by name |

The `ColleagueDto` gains an `Id` field; the handler maps it from the updated `ColleagueRecord`.

**Commands** (`Features/Colleagues/Commands/`):

| File | Input | Returns | What it does |
|---|---|---|---|
| `AddColleague.cs` | `string Name`, `string HexColor` | `OneOf<AddColleagueResponse, ApiError>` | Creates a new Colleague row via repository |
| `UpdateColleague.cs` | `int Id`, `string Name`, `string HexColor` | `OneOf<UpdateColleagueResponse, ApiError>` | Updates Name and HexColor for the given Id |

Both handlers catch all exceptions and map to `ApiError`; neither propagates.

### Domain (`Domain/`)

No changes — `Colleague` entity already has `Id`, `Name`, and `HexColor`.

### Persistence (`Persistence/`)

**`ColleagueRecord`** in `MobileApp.Contracts/Repositories/IColleagueRepository.cs`:
- Add `int Id` as the first positional parameter of the record

**`IColleagueRepository`** — add two methods:
- `Task AddAsync(string name, string hexColor, CancellationToken cancellationToken = default)`
- `Task UpdateAsync(int id, string name, string hexColor, CancellationToken cancellationToken = default)`

**`ColleagueRepository`** — implement the two new methods using `DeskWatchDbContext`:
- `AddAsync`: construct a new `Colleague` entity, add to context, save changes
- `UpdateAsync`: find by Id, update `Name` and `HexColor`, save changes (throw if not found, let handler map to `ApiError`)

## Implementation order

1. **Contracts** — add `Id` to `ColleagueRecord`; add `AddAsync` and `UpdateAsync` to `IColleagueRepository`
2. **Persistence** — implement the two new repository methods in `ColleagueRepository`
3. **Application** — update `GetColleaguesQuery` response DTO; scaffold `AddColleague` and `UpdateColleague` commands
4. **Presentation — rename** — rename existing Colleagues page/ViewModel files to Log, update class names, DI registrations, and AppShell tab
5. **Presentation — new Colleagues page** — scaffold `ColleaguesPage` + `ColleaguesPageViewModel` + `ColleagueFormBottomSheet` + `ColleagueFormViewModel`
6. **Build verification** — run `dotnet build` + `dotnet test` and route failures to the responsible agent

## Open questions

- **Swatch palette**: no specific colors were specified — the implementation agent should choose a balanced set of 10–12 visually distinct colors that work in both light and dark mode (e.g., a material-design-style palette). Confirm with the user after first implementation if the palette needs adjustment.
- **List ordering**: the plan assumes alphabetical by name; confirm if a different order (e.g., most recently added first) is preferred.
- **Duplicate names**: no validation against duplicate names is specified — the plan omits it; add if needed.
