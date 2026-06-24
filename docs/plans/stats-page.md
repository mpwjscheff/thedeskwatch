# Stats Page — Implementation Plan

## Overview

A new Stats tab (last position in the tab bar) that displays three humorous, data-driven
office statistics computed over all historical `DeskDeparture` records. Each statistic is
presented as a vertical card: a Syncfusion chart on top, followed by a FontAwesome icon and
a one-line caption below. All three cards appear on a single scrollable page.

## Acceptance criteria

- [ ] A "Stats" tab appears last in the tab bar with a chart icon and the label "Stats"
- [ ] The page renders three stat cards in order: The Great Exodus, Coffee O'Clock, The Post-Lunch Drift
- [ ] Each card shows a Syncfusion column chart, a FontAwesome icon, and a short caption
- [ ] The Great Exodus card prominently displays the peak escape time (e.g. "Peak escape time: 3:14 PM")
- [ ] Coffee O'Clock labels the tallest bar(s) as "caffeine event"
- [ ] The Post-Lunch Drift compares post-lunch departures vs the rest of the day and labels the result "food coma migration" when post-lunch absences are higher
- [ ] All statistics are computed over the full departure history
- [ ] The page is registered in the tab bar and navigable without any route parameter

## Architecture decisions

- All aggregation logic lives in a single `GetStatsQuery` handler in the Application layer; the
  ViewModel does no math, only formatting and chart-model mapping.
- A new `IDeskDepartureRepository` is introduced in `MobileApp.Contracts` with a single
  `GetAllAsync()` method, implemented in `Persistence` — the Application layer never touches
  `DeskWatchDbContext` directly.
- `Syncfusion.Maui.Toolkit` is added as a NuGet package to `TheDeskWatch.MobileApp.csproj`
  and initialized in `MauiProgram.cs` via `UseSyncfusionToolkit()`. No other project
  references it.
- FontAwesome 6 Solid is already set up; new icons are referenced with the existing
  `FontAwesome6Solid` alias — no additional font setup needed.
- Chart data is exposed from the ViewModel as `ObservableCollection<ChartDataPoint>` where
  `ChartDataPoint` is a simple value object defined in the ViewModel file (presentation-only,
  not a shared DTO).

## Layers

### Presentation (`MobileApp/`)

**New files to create:**
- `Pages/Stats/Pages/StatsPage.xaml` + `StatsPage.xaml.cs`
- `Pages/Stats/ViewModels/StatsPageViewModel.cs`

**ViewModel shape:**

Observable properties:
- `GreatExodusChartData` — `ObservableCollection<ChartDataPoint>` (15-minute slot label → departure count, covering working hours)
- `PeakEscapeTimeLabel` — `string` formatted as "Peak escape time: 3:14 PM"
- `CoffeeOClockChartData` — `ObservableCollection<ChartDataPoint>` (hour label → departure count)
- `CaffeineEventThreshold` — `double` used in XAML to drive the data-label visibility rule ("caffeine event" appears on bars at or above this value)
- `PostLunchDriftChartData` — `ObservableCollection<ChartDataPoint>` (two entries: "Post-lunch" and "Rest of day")
- `IsFoodComaMigration` — `bool`, true when post-lunch count exceeds rest-of-day average; drives a conditional label on the card

Commands:
- `LoadStatsCommand` — async, fires `GetStatsQuery`, populates all observable properties; called from `OnAppearing` in code-behind

Constructor sets `Title = "Stats"`.

**Card layout (repeated three times, each in a styled `Frame`):**
- `SfCartesianChart` with a `ColumnSeries` bound to the relevant `ChartDataPoint` collection;
  `XBindingPath = "Label"`, `YBindingPath = "Value"`
- Below the chart: a horizontal stack containing a `Label` with a FontAwesome glyph and a
  `Label` with the short caption text, both using resource tokens for size and color

**FontAwesome icons per card:**
- The Great Exodus: `&#xf2f5;` (`fa-right-from-bracket`) — "When the herd bolts"
- Coffee O'Clock: `&#xf7b6;` (`fa-mug-hot`) — "Confirmed by data"
- The Post-Lunch Drift: `&#xf2e7;` (`fa-utensils`) — "The food coma is real"

**Tab bar icon:** `&#xf080;` (`fa-chart-bar`) with the label "Stats"

**DI + navigation:**
- `MauiProgram.cs`: `AddTransient<StatsPage>()` and `AddTransient<StatsPageViewModel>()`
- `AppShell.xaml`: add a `ShellContent` for `StatsPage` as the last entry in the `TabBar`,
  matching the icon/color style of the existing two tabs
- No `GoToAsync` route needed (tab navigation only)

**Services / Helpers needed:**
- None beyond what already exists

### Application (`Application/`)

**Queries** (`Features/Stats/Queries/`):

| File | Parameters | Response DTO | What it returns |
|---|---|---|---|
| `GetStatsQuery.cs` | none | `GetStatsResponse` | All three aggregated statistics |

`GetStatsResponse` contains:
- `GreatExodus` — nested record with `PeakTime` (`TimeOnly`) and a list of `(string SlotLabel, int Count)` covering every 15-minute slot in the day that has at least one departure
- `CoffeeOClock` — list of `(string HourLabel, int Count)` for each hour of the day, plus `int CaffeineEventThreshold` (the count of the tallest bar, used to flag bars that match it)
- `PostLunchDrift` — record with `int PostLunchCount` (departures between 12:00–14:00), `double RestOfDayAvgPerHour`, and `bool IsFoodComaMigration`

The handler loads all `DeskDeparture` records via `IDeskDepartureRepository.GetAllAsync()`,
performs all aggregation in memory, and returns the response wrapped in `OneOf<GetStatsResponse, ApiError>`.
Any exception is caught internally and mapped to `ApiError`.

### Domain (`Domain/`)

No new models. The existing `DeskDeparture` entity (with `DepartedAt` and `ColleagueId`) is
sufficient for all three statistics.

### Persistence (`Persistence/`)

- New repository implementation `DeskDepartureRepository` that loads all `DeskDeparture`
  rows from `DeskWatchDbContext.DeskDepartures` and returns them as an enumerable.
- Registered in `MauiProgram.cs` as `AddScoped<IDeskDepartureRepository, DeskDepartureRepository>()`.

### Contracts (`MobileApp.Contracts/`)

**New interface:**
- `IDeskDepartureRepository` in `Repositories/` — single method `GetAllAsync(CancellationToken = default)` returning `Task<IEnumerable<DeskDeparture>>`

## Implementation order

1. **Contracts** — add `IDeskDepartureRepository` to `MobileApp.Contracts/Repositories/`
2. **Persistence** — implement `DeskDepartureRepository`; wire DI in `MauiProgram.cs`
3. **Application** — scaffold `GetStatsQuery` with handler; run `dotnet test` to confirm architecture rules pass
4. **Presentation** — add Syncfusion NuGet, call `UseSyncfusionToolkit()` in `MauiProgram.cs`, scaffold `StatsPage` + `StatsPageViewModel`, add tab bar entry in `AppShell.xaml`

## Open questions

- **Syncfusion license**: `Syncfusion.Maui.Toolkit` is open-source (MIT) and requires no
  license key, but confirm the correct package name before adding — the commercial
  `Syncfusion.Maui.Charts` package has a different name and requires registration.
- **"Caffeine event" threshold**: the plan uses the tallest bar's count as the threshold,
  meaning only the single tallest bar (or ties) gets the label. Adjust if you prefer a
  percentage-based threshold instead.
- **Working-hours filter for The Great Exodus**: the plan includes all hours that have at
  least one departure. If you want to restrict the chart to e.g. 08:00–18:00, that can be
  added to the query.
