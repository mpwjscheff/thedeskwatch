---
name: plan-feature
description: >
  Runs a one-question-at-a-time interview to gather full requirements for a new feature in
  TheDeskWatch, then writes a structured implementation plan as a markdown file covering
  every architectural layer: Presentation (MAUI pages, ViewModels, Services, Helpers),
  Application (LiteBus commands and queries), Domain (models), Persistence (repositories),
  and Contracts (platform capability interfaces). Use this skill whenever the user describes
  something they want to build — a new page, a background feature, a data flow, a widget, an
  integration — even when the request is brief or vague. Trigger proactively when a feature
  request would benefit from clarification before any code is written. Also triggers on
  explicit invocations: /plan-feature, "plan this feature", "let's plan X", "help me spec X",
  "I want to implement X", "we need to build X".
---

# Feature Planning — TheDeskWatch

Gather requirements for a new feature through a focused interview, then produce a complete
implementation plan that a developer can follow layer by layer.

## Architecture reference

TheDeskWatch is a .NET 10 MAUI app:

| Layer | Project | What lives here |
|---|---|---|
| Presentation | `MobileApp/` | XAML pages, code-behind, ViewModels (CommunityToolkit.Mvvm), `Services/` (Contracts impls), `Helpers/` (internal MAUI abstractions) |
| Application | `Application/` | LiteBus commands + queries under `Features/{Feature}/` |
| Domain | `Domain/` | Domain models |
| Persistence | `Persistence/` | Repositories, data access |
| Contracts | `MobileApp.Contracts/` | Platform capability interfaces (`IFileService`, etc.) |

Key conventions from CLAUDE.md:
- ViewModel backing a Page → `{Name}PageViewModel`; backing a reusable View → `{Name}ViewModel`
- Commands and queries return `OneOf<T, ApiError>`; never propagate exceptions to callers
- Every color / spacing / font token lives in `Resources/Styles/` — no inline literals in XAML
- Every page and ViewModel is DI-registered in `MauiProgram.cs`

---

## Conduct the interview

Ask **exactly one question per message**. Wait for the answer before continuing. Each answer
shapes what you ask next — some questions become irrelevant, others spawn follow-ups.

There is no fixed script; the flow below is a guide. Adapt freely.

The user will always arrive with a specific feature already in mind — do not ask them to
describe what they want to build. Extract the feature from their message and immediately
start clarifying the technical details. Your first question should be the most important
unknown that the user's message left open.

**Presentation layer questions** (ask what's relevant)
- Is this a new screen, or does it extend an existing one?
  - If new: what is it called? What does the user see on it?
  - If existing: which page? What's being added?
- How does the user get to this screen? (tab bar, button, shell route, notification tap, deep link…)
- Does the UI need to react to data changes in real time, or is it load-once?
- Are there reusable sub-views that would appear elsewhere in the app?

**Application layer questions**
- What data does this feature need to display or work with?
- What actions can the user (or the system) take that change state? (These become Commands.)
- Are there any business rules, validation conditions, or constraints on those actions?

**Domain questions** (only if new models seem likely)
- Does this feature introduce new concepts/entities that don't exist in the codebase yet?
- If yes: what are they, and what are their key properties?

**Persistence questions**
- Where does the data live — local SQLite, a remote API, device preferences, or somewhere else?
- Are new tables, repositories, or data sources needed?

**Contracts questions** (only if platform capabilities seem needed)
- Does this feature need direct access to platform capabilities — file system, camera,
  notifications, location, permissions, biometrics?

**Closing — acceptance criteria**
- What are the main error states or edge cases the feature must handle?
- What does "done" look like? (What should a tester be able to verify?)

**When to stop asking**: after roughly 8–15 questions you'll have enough to write the plan.
Stop when further questions would be redundant or speculative. If something is genuinely
unclear, note it as an open question in the plan rather than blocking the interview.

---

## Write the implementation plan

Once the interview is complete, write the plan to:

```
docs/plans/<feature-kebab-case>.md
```

Create `docs/plans/` if it doesn't exist. Derive the file name from the feature being built
(e.g., `task-creation.md`, `home-dashboard.md`, `offline-sync.md`, `push-notifications.md`).

### Writing rules

- **No code snippets.** Describe everything in prose, bullet points, or tables — never include
  C#, XAML, or any other code blocks. Name the files, methods, properties, and types by name,
  but do not show their implementation.
- Keep each layer section concise: a handful of bullet points or a small table is enough.

### Plan template

```markdown
# <Feature Name> — Implementation Plan

## Overview
What this feature is, what problem it solves, and how it fits into the app.

## Acceptance criteria
- [ ] <verifiable condition>
- [ ] …

## Architecture decisions
Choices made during the interview: storage strategy, whether Contracts are needed, key
design trade-offs.

## Layers

### Presentation (`MobileApp/`)

**New files to create:**
- `Pages/{Feature}/Pages/{PageName}.xaml` + `{PageName}.xaml.cs`
- `Pages/{Feature}/ViewModels/{PageName}PageViewModel.cs`
- `Pages/{Feature}/Views/{ViewName}.xaml` (if reusable sub-views are needed)

**ViewModel shape:**
- `[ObservableProperty]` fields: …
- `[RelayCommand]` methods: …
- Constructor sets `Title = "…"`

**DI + navigation:**
- `MauiProgram.cs`: register page and ViewModel with `AddTransient<>()`
- `AppShell.xaml.cs`: register route if navigable via `GoToAsync`

**Services / Helpers needed:**
- …

### Application (`Application/`)

**Commands** (`Features/{Feature}/Commands/`):
| File | Input | Returns | What it does |
|---|---|---|---|
| `{CommandName}.cs` | … | `OneOf<…, ApiError>` | … |

**Queries** (`Features/{Feature}/Queries/`):
| File | Parameters | Response DTO | What it returns |
|---|---|---|---|
| `{QueryName}.cs` | … | `{QueryName}Response` | … |

**Feature services** (if shared logic spans multiple use cases):
- …

### Domain (`Domain/`)
New models (skip if none):
- `{ModelName}`: properties and their types

### Persistence (`Persistence/`)
- New tables or schema changes
- Repository interfaces and implementations needed

### Contracts (`MobileApp.Contracts/`)
New platform capability interfaces (skip if none):
- `I{Name}Service`: method signatures

## Implementation order
1. Domain models — no dependencies, safe to start
2. Contracts interfaces (if any) — pure .NET, no MAUI ref needed
3. Persistence layer — depends only on Domain
4. Application commands and queries — depend on Domain + Contracts
5. Presentation layer — depends on Application

## Open questions
Things that weren't resolved in the interview and need a decision during implementation.
```

### After writing the file

Tell the user:
1. The path to the plan file (as a relative path from the project root)
2. A one-line summary of what the plan covers
3. Whether any open questions need resolving before work starts
4. Which scaffold skills to reach for first (e.g. `scaffold-page`, `scaffold-command`, `scaffold-query`)
