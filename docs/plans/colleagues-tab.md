# Colleagues Tab — Implementation Plan

## Overview

A new "Colleagues" tab added to the app's Shell tab bar (creating the tab bar if one does not
already exist). The tab hosts a scrollable 2-column grid of circular, bubble-styled cards —
one per colleague — each showing the colleague's name on a unique background color. Tapping a
bubble plays a brief pulse animation and increments an in-memory stand-up counter for that
colleague. Data is hard-coded in the Application layer; persistence and color management from
a database are deferred to a future iteration.

## Acceptance criteria

- [ ] A "Colleagues" tab appears in the app's tab bar and is reachable from any other tab.
- [ ] The Colleagues page displays all colleagues returned by the query.
- [ ] Bubbles are circular, display the colleague's name legibly, and each has a distinct background color sourced from the query response.
- [ ] Bubbles are arranged in a 2-column grid and the page scrolls vertically when the list overflows.
- [ ] Tapping a bubble plays a brief pulse animation (scale-up then scale-back) confirming the interaction.
- [ ] Each tap increments the in-memory stand-up count for that colleague (not displayed; verified in code only).
- [ ] No hard-coded literal colors or sizes appear in XAML — all design tokens live in `Resources/Styles/`.

## Architecture decisions

- **No new Domain models**: colleagues are represented entirely as a DTO inside the Application
  layer query response. No `Domain/` changes are needed.
- **No Persistence layer changes**: the query handler returns a hard-coded list. A repository
  will be introduced in a future iteration.
- **No Contracts interfaces**: the feature requires no platform capabilities.
- **Colors as hex strings in the DTO**: the `ColleagueDto` carries the colleague's color as a
  hex string. The presentation layer converts this to a `Color` at bind time so the Application
  layer stays MAUI-agnostic.
- **In-memory stand-up counts in the ViewModel**: a private `Dictionary<string, int>` keyed on
  colleague name (or ID once persistence is added) lives in the ViewModel. It is not exposed
  as a bindable property.
- **Reusable bubble view**: the circular bubble is extracted into a `ColleagueBubbleView`
  ContentView so it can be reused if colleagues appear elsewhere in the app.

## Layers

### Presentation (`MobileApp/`)

**New files to create:**
- `Pages/Colleagues/Pages/ColleaguesPage.xaml` + `ColleaguesPage.xaml.cs`
- `Pages/Colleagues/ViewModels/ColleaguesPageViewModel.cs`
- `Pages/Colleagues/Views/ColleagueBubbleView.xaml` + `ColleagueBubbleView.xaml.cs`

**ViewModel shape (`ColleaguesPageViewModel`):**
- Constructor sets `Title = "Colleagues"` and loads data on initialisation.
- `[ObservableProperty]` field: `ObservableCollection<ColleagueDto> colleagues` — the grid's
  item source.
- Private `Dictionary<string, int> _standUpCounts` — keyed by colleague name; incremented on
  each tap.
- `[RelayCommand]` method `RegisterStandUp(ColleagueDto colleague)` — increments the counter
  for the given colleague and raises a one-shot animation trigger (see below).
- Data loaded by sending a `GetColleaguesQuery` via `IQueryMediator` injected through the
  constructor.

**Animation:**
- `ColleagueBubbleView` exposes a method `PlayTapAnimation` (scale to 1.15 over 80 ms, then
  back to 1.0 over 80 ms using MAUI's `ScaleTo`).
- The command in the ViewModel raises an event or uses a `WeakReferenceMessenger` message
  (`CommunityToolkit.Mvvm.Messaging`) so the view can call `PlayTapAnimation` without the
  ViewModel holding a view reference.

**Layout:**
- `ColleaguesPage` contains a `ScrollView` wrapping a `Grid` with two fixed-width columns.
  Rows are added dynamically via `CollectionView` or `BindableLayout` bound to `Colleagues`.
- Each cell is a `ColleagueBubbleView` whose `BindingContext` is a `ColleagueDto`.

**Bubble styling (`ColleagueBubbleView`):**
- Shape: equal `WidthRequest` / `HeightRequest` with `CornerRadius` equal to half the size,
  producing a perfect circle.
- Background: filled with the colleague's color (bound from `ColleagueDto.HexColor`);
  a `LinearGradientBrush` overlay (semi-transparent white from top-left to transparent
  bottom-right) gives a bubble sheen without requiring radial gradient support.
- Text: colleague's `Name` centered, white foreground, `FontSize` token from `Styles.xaml`,
  with `LineBreakMode="TailTruncation"` for long names.
- Shadow: a subtle `Shadow` element to lift the bubble off the background.

**Tokens to add in `Resources/Styles/`:**
- `BubbleSize` (e.g. numeric resource for width/height) in `Styles.xaml`.
- `BubbleFontSize` in `Styles.xaml`.
- `BubbleNameTextColor` (white for both light and dark) in `Colors.xaml`.
- Grid spacing / padding values as named resources if not already present.

**DI + navigation (`MauiProgram.cs`):**
- Register `ColleaguesPage` and `ColleaguesPageViewModel` both with `AddTransient<>()`.
- No shell route registration is needed — the tab is accessed directly from the tab bar, not
  via `GoToAsync`.

**Shell (`AppShell.xaml` + `AppShell.xaml.cs`):**
- If the shell currently uses `<ShellContent>` at the top level with no `<TabBar>`, wrap
  existing content in a `<TabBar>` and add a second `<Tab>` for Colleagues.
- If a `<TabBar>` already exists, add a new `<Tab Title="Colleagues">` containing a
  `<ShellContent ContentTemplate="{DataTemplate pages:ColleaguesPage}">`.
- Set a tab bar icon for Colleagues (use an existing resource or a placeholder; icon sourcing
  is an open question — see below).

**Helpers needed:** none beyond what already exists.

---

### Application (`Application/`)

**Queries** (`Features/Colleagues/Queries/`):

| File | Parameters | Response DTO | What it returns |
|---|---|---|---|
| `GetColleagues.cs` | none | `GetColleaguesResponse` | Hard-coded list of `ColleagueDto` |

- `GetColleaguesQuery` — a public, non-sealed record implementing the appropriate LiteBus
  query interface.
- `GetColleaguesResponse` — a public sealed record containing an `IReadOnlyList<ColleagueDto>`.
- `ColleagueDto` — a public sealed record nested inside `GetColleaguesResponse` with two
  properties: `Name` (string) and `HexColor` (string, e.g. `"#E74C3C"`).
- Handler — public sealed class; `HandleAsync` returns a hard-coded set of 6–8 colleagues with
  varied names and hex colors, wrapped in `OneOf<GetColleaguesResponse, ApiError>`.

**Commands:** none at this stage (stand-up count is in-memory only).

**Feature services:** none (single query, no shared logic).

---

### Domain (`Domain/`)

No changes. Colleagues are represented as DTOs in the Application layer only.

---

### Persistence (`Persistence/`)

No changes. The query handler hard-codes its data; a repository will be introduced when
database persistence is added.

---

### Contracts (`MobileApp.Contracts/`)

No changes. No platform capabilities are required.

---

## Implementation order

1. **Application query** — `GetColleagues.cs` with hard-coded data; no dependencies.
2. **Presentation — bubble view** — `ColleagueBubbleView` with styling tokens; depends only
   on the DTO shape being known.
3. **Presentation — page + ViewModel** — `ColleaguesPage` and `ColleaguesPageViewModel`;
   depends on the query and the bubble view.
4. **Shell wiring** — add/update `AppShell.xaml` to include the Colleagues tab.
5. **DI registration** — update `MauiProgram.cs`.
6. **Build verification** — run `dotnet build` + `dotnet test` to confirm no regressions.

## Open questions

- **Tab bar icon**: what icon should appear on the Colleagues tab bar item? A placeholder can
  be used initially, but a final asset is needed before shipping.
- **Bubble size**: is a specific size preferred (e.g. 100 × 100 pt), or should it be derived
  from the screen width so exactly two columns fill the viewport edge-to-edge?
- **Animation signal**: the plan proposes `WeakReferenceMessenger` to decouple ViewModel from
  View for the tap animation. If the team prefers a different pattern (e.g. a bindable
  `IsAnimating` flag polled by a behavior), this should be decided before implementation.
- **Existing Shell structure**: the implementor must inspect `AppShell.xaml` to determine
  whether a `<TabBar>` already exists before making changes.
