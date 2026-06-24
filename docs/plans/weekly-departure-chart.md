# Weekly Departure Chart — Implementation Plan

## Overview

Add a bar chart to the bottom of the Stats page that visualises how many desk-departure
events occurred on each day of the previous Monday–Friday work week. The chart uses the
same `SfCartesianChart` control from `Syncfusion.Maui.Toolkit` that is already in use on
the Stats page, so no new dependencies are required.

## Acceptance criteria

- [ ] A new bar chart appears as the last element on the Stats page.
- [ ] The x-axis shows the five weekday labels for the previous work week (Mon–Fri).
- [ ] The y-axis shows the total count of desk-departure events for each of those days.
- [ ] If no data exists for a given day, its bar is absent or shows zero.
- [ ] The chart is styled consistently with the existing Stats page charts (tokens from `Resources/Styles/`).
- [ ] The chart loads its data alongside the rest of the Stats page (no separate user action required).

## Architecture decisions

- **Separate query**: The new data slice (previous-week aggregation) is distinct enough from
  the existing all-time `GetStatsQuery` to warrant its own query —
  `GetWeeklyDepartureStatsQuery`. This keeps each query focused and avoids bloating the
  existing response DTO.
- **No new repository method**: `IDeskDepartureRepository.GetAllAsync()` already returns every
  departure; the query handler filters and groups in memory. A dedicated repository method
  would be premature given the small dataset size.
- **No new Domain models**: `DeskDeparture` (with `DepartedAt` and `ColleagueId`) is
  sufficient to compute the aggregation.
- **No new Contracts**: no platform capability is required.

## Layers

### Presentation (`MobileApp/`)

**Files to modify:**
- `Pages/Stats/Pages/StatsPage.xaml` — append the new `SfCartesianChart` block after the
  existing last stat element.
- `Pages/Stats/ViewModels/StatsPageViewModel.cs` — inject `IQueryMediator`, add an
  `[ObservableProperty]` for the weekly chart data, and call the new query during load.

**ViewModel shape additions:**
- One `[ObservableProperty]` field of type `ObservableCollection<WeeklyDayDataPointDto>`
  (or a plain `List` promoted to observable) named something like `weeklyDepartureSeries`.
- The existing load command (or `OnAppearing` trigger) dispatches `GetWeeklyDepartureStatsQuery`
  and assigns the result to the observable property.

**XAML additions (StatsPage.xaml):**
- A new `SfCartesianChart` at the bottom of the scroll/stack layout.
- `CategoryAxis` on the x-axis bound to the day-label property of each data point.
- `NumericalAxis` on the y-axis.
- A single `ColumnSeries` bound to `weeklyDepartureSeries`, with its x-value path pointing
  to the day label and y-value path pointing to the departure count.
- Chart title, axis labels, and color tokens sourced exclusively from `Resources/Styles/`
  — no inline literals.

**Services / Helpers needed:** none beyond what already exists.

### Application (`Application/`)

**New query** (`Features/Stats/Queries/GetWeeklyDepartureStatsQuery.cs`):

| File | Parameters | Response DTO | What it returns |
|---|---|---|---|
| `GetWeeklyDepartureStatsQuery.cs` | none (or optional `CancellationToken`) | `GetWeeklyDepartureStatsResponse` | One `WeeklyDayDataPointDto` per weekday (Mon–Fri) of the previous work week, each holding a short day label and a total departure count |

**Handler logic (inside the same file):**
- Inject `IDeskDepartureRepository`.
- Call `GetAllAsync()` to retrieve all departures.
- Determine the date range for the previous work week: the Monday through Friday of the
  calendar week immediately before the current week.
- Filter departures whose `DepartedAt` date falls within that range.
- Group by calendar date, then map each group to a `WeeklyDayDataPointDto` with a short
  label (e.g. "Mon", "Tue") and the group count.
- Ensure all five weekdays appear in the result — days with no data get a count of zero.
- Return `OneOf<GetWeeklyDepartureStatsResponse, ApiError>`; wrap any exception in `ApiError`.

**Response DTO (nested in the same file):**
- `GetWeeklyDepartureStatsResponse` — a `public sealed record` holding
  `IReadOnlyList<WeeklyDayDataPointDto> Days`.
- `WeeklyDayDataPointDto` — a `public sealed record` with `string Label` and `int Count`.

### Domain (`Domain/`)

No new models needed.

### Persistence (`Persistence/`)

No schema changes and no new repository methods needed. The handler reuses
`IDeskDepartureRepository.GetAllAsync()`.

### Contracts (`MobileApp.Contracts/`)

No new platform capability interfaces needed.

## Implementation order

1. **Application layer** — add `GetWeeklyDepartureStatsQuery.cs` with its handler and
   response DTOs. This has no dependencies on the presentation layer.
2. **Presentation layer** — update `StatsPageViewModel` to call the new query, then update
   `StatsPage.xaml` to render the chart. Depends on step 1 to compile.
3. **Build verification** — run `dotnet build TheDeskWatch.slnx` and `dotnet test` to
   confirm no regressions.

## Open questions

- **Chart title copy**: no title string was agreed during the interview. Suggest "Last
  Week's Departures" — confirm before implementation.
- **"Previous week" boundary on Mondays**: if the app is used on a Monday before any data
  exists for the new week, the previous week is unambiguous. If run over a weekend, decide
  whether "previous week" means the week just ended (Sat/Sun → last Mon–Fri) or the week
  before that.
