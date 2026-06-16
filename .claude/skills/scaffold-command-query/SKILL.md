---
name: scaffold-command-query
description: >
  Scaffolds Application layer Commands and Queries for TheDeskWatch following the LiteBus mediator
  pattern. Creates a .cs file with complete, architecture-compliant boilerplate under
  {Feature}/Commands/ or {Feature}/Queries/. LiteBus handlers self-register via
  assembly scanning so no DI wiring is needed.

  Use this skill whenever the user wants to add business logic to the Application layer — "add a
  command to save X", "create a query to fetch Y", "scaffold a use case for Z", "I need to handle
  [action]", or when they name an operation like "SavePreferences", "GetDashboardData", or
  "DeleteTask". Also trigger when the user asks for both a command and a query together for the
  same feature.
---

## Identify what to create

Extract from the user's request:

1. **Feature name** — the domain folder (e.g., `Clock`, `Settings`, `Home`). Ask if unclear.
2. **Operation name** — PascalCase, no suffix (e.g., `SaveClockSettings`, `GetHomeData`). Infer from
   the description; confirm if ambiguous.
3. **Type** — infer from the leading verb:
   - **Command**: Save, Create, Update, Delete, Send, Process, Request, Execute, Mark, Set, Reset, Toggle
   - **Query**: Get, Fetch, Load, List, Read, Find, Search, Retrieve

If type genuinely can't be inferred, ask before creating. Prefer inferring — most requests make it obvious.

## Paths

| Type    | Path |
|---------|------|
| Command | `src/TheDeskWatch.Application/{Feature}/Commands/{OperationName}.cs` |
| Query   | `src/TheDeskWatch.Application/{Feature}/Queries/{OperationName}.cs` |

Create any intermediate directories as needed.

## Command template

```csharp
using System.Diagnostics;
using LiteBus.Commands.Abstractions;
using OneOf;
using OneOf.Types;

namespace TheDeskWatch.Application.{Feature}.Commands;

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

## Query template

```csharp
using LiteBus.Queries.Abstractions;
using OneOf;

namespace TheDeskWatch.Application.{Feature}.Queries;

public class {OperationName}Query : IQuery<OneOf<{OperationName}QueryResponse, ApiError>>;

public sealed record {OperationName}QueryResponse
{
    // TODO: add response properties
}

public sealed class {OperationName}Handler()
    : IQueryHandler<{OperationName}Query, OneOf<{OperationName}QueryResponse, ApiError>>
{
    public async Task<OneOf<{OperationName}QueryResponse, ApiError>> HandleAsync(
        {OperationName}Query query, CancellationToken cancellationToken = new())
    {
        // TODO: implement
        await Task.CompletedTask;
        return new {OperationName}QueryResponse();
    }
}
```

## Architecture rules to double-check

**Commands:**
- Command class: `public sealed`, empty body (`{ }`)
- Handler class: `internal sealed`, primary constructor (dependencies as constructor params)
- `CancellationToken cancellationToken = default` — never `new()`
- Handler body: compact single-line style — `try { ... }` / `catch (Exception e) { Debug.WriteLine(e); return new ApiError(); }` / `return new Success();`

**Queries:**
- Query class: `public` — NOT sealed — using interface-shorthand syntax (`; ` at the end, no `{ }`)
- Response: `public sealed record`
- Handler class: `public sealed`, primary constructor, named `{OperationName}Handler` (no "Query" suffix)
- `CancellationToken cancellationToken = new()` — never `default`
- No top-level try/catch in the handler; private helpers are responsible for swallowing their own errors

**Both:**
- `ApiError` is a project-defined type — no `using` needed for it
- LiteBus handlers self-register via `RegisterFromAssembly` in `MauiProgram.cs` — no explicit DI call required

## Prerequisites (flag if missing)

If the Application project doesn't yet reference LiteBus or OneOf, add to
`src/TheDeskWatch.Application/TheDeskWatch.Application.csproj`:

```xml
<PackageReference Include="LiteBus.Commands.Abstractions" Version="*" />
<PackageReference Include="LiteBus.Queries.Abstractions" Version="*" />
<PackageReference Include="OneOf" Version="*" />
```

If `ApiError` isn't defined anywhere in `TheDeskWatch.Application`, tell the user — don't create it
automatically, as its shape is a domain decision.

## After creating the file

Tell the user:
- The path of the file created
- Which TODOs need to be filled in (implementation body, response properties)
- Any missing prerequisites flagged above
