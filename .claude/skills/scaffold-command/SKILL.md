---
name: scaffold-command
description: >
  Scaffolds an Application layer Command for TheDeskWatch following the LiteBus mediator pattern.
  Creates a .cs file with complete, architecture-compliant boilerplate under {Feature}/Commands/.
  LiteBus handlers self-register via assembly scanning so no DI wiring is needed.

  Use this skill whenever the user wants to add a write/mutation use case to the Application layer —
  "add a command to save X", "scaffold a use case for Z", "I need to handle [action]", or when they
  name an operation whose verb mutates state: Save, Create, Update, Delete, Send, Process, Request,
  Execute, Mark, Set, Reset, Toggle (e.g. "SavePreferences", "DeleteTask"). For read operations
  (Get/Fetch/List/…) use the scaffold-query skill instead; if the user asks for both a command and a
  query, run both skills.
---

## Identify what to create

Extract from the user's request:

1. **Feature name** — the domain folder (e.g., `Clock`, `Settings`, `Home`). Ask if unclear.
2. **Operation name** — PascalCase, no suffix (e.g., `SaveClockSettings`, `DeleteTask`). Infer from
   the description; confirm if ambiguous.
3. **Confirm it's a command** — commands mutate state. If the operation actually reads state, use the **scaffold-query** skill instead.

## Path

| Type    | Path |
|---------|------|
| Command | `src/TheDeskWatch.Application/Features/{Feature}/Commands/{OperationName}.cs` |

Create any intermediate directories as needed.

## Command template

```csharp
using System.Diagnostics;
using LiteBus.Commands.Abstractions;
using OneOf;
using OneOf.Types;

namespace TheDeskWatch.Application.Features.{Feature}.Commands;

public sealed class {OperationName}Command : ICommand<OneOf<Success, ApiError>> { }

internal sealed class {OperationName}CommandHandler()
    : ICommandHandler<{OperationName}Command, OneOf<Success, ApiError>>
{
    public async Task<OneOf<Success, ApiError>> HandleAsync(
        {OperationName}Command command, CancellationToken cancellationToken = default)
    {
        try { /* TODO: implement */ await Task.CompletedTask; }
        catch (Exception e) { Debug.WriteLine(e); return new ApiError(); }
        return new Success();
    }
}
```

## Easy-to-miss details

The template already encodes the visibility/structure conventions (`Architecture.Tests` enforces them at build). The points not obvious from the template:

- The `Features.` segment in the namespace is required — the `Architecture.Tests` regex matches only `…Application.Features.*.Commands`.
- `CancellationToken cancellationToken = default` — never `new()` (the query skill uses `new()`; don't mix them up).
- `ApiError` is a project-defined type — no `using` needed for it.

## Prerequisites (flag if missing)

If the Application project doesn't yet reference LiteBus or OneOf, add to
`src/TheDeskWatch.Application/TheDeskWatch.Application.csproj`:

```xml
<PackageReference Include="LiteBus.Commands.Abstractions" Version="*" />
<PackageReference Include="OneOf" Version="*" />
```

If `ApiError` isn't defined anywhere in `TheDeskWatch.Application`, tell the user — don't create it
automatically, as its shape is a domain decision.

## After creating the file

Tell the user:
- The path of the file created
- Which TODOs need to be filled in (implementation body)
- Any missing prerequisites flagged above
