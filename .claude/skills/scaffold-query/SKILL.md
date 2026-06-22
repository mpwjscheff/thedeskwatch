---
name: scaffold-query
description: >
  Scaffolds an Application layer Query for TheDeskWatch following the LiteBus mediator pattern.
  Creates a .cs file with complete, architecture-compliant boilerplate under {Feature}/Queries/.
  LiteBus handlers self-register via assembly scanning so no DI wiring is needed.

  Use this skill whenever the user wants to add a read use case to the Application layer — "create a
  query to fetch Y", "scaffold a use case to load Z", or when they name an operation whose verb reads
  state: Get, Fetch, Load, List, Read, Find, Search, Retrieve (e.g. "GetDashboardData",
  "ListTasks"). For write/mutation operations (Save/Create/Delete/…) use the scaffold-command skill
  instead; if the user asks for both a command and a query, run both skills.
---

## Identify what to create

Extract from the user's request:

1. **Feature name** — the domain folder (e.g., `Clock`, `Settings`, `Home`). Ask if unclear.
2. **Operation name** — PascalCase, no suffix (e.g., `GetHomeData`, `ListTasks`). Infer from
   the description; confirm if ambiguous.
3. **Confirm it's a query** — queries read state. If the operation actually mutates state, use the **scaffold-command** skill instead.

## Path

| Type  | Path |
|-------|------|
| Query | `src/TheDeskWatch.Application/Features/{Feature}/Queries/{OperationName}.cs` |

Create any intermediate directories as needed.

## Query template

```csharp
using LiteBus.Queries.Abstractions;
using OneOf;

namespace TheDeskWatch.Application.Features.{Feature}.Queries;

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

## Easy-to-miss details

The template already encodes the visibility/structure conventions (`Architecture.Tests` enforces them at build). The points not obvious from the template:

- The `Features.` segment in the namespace is required — the `Architecture.Tests` regex matches only `…Application.Features.*.Queries`.
- The query class is `public` but **not** `sealed` (unlike the command), using interface-shorthand syntax (`;`, no body).
- The handler is named `{OperationName}Handler` — no `Query` suffix (the command handler keeps its `Command` suffix).
- `CancellationToken cancellationToken = new()` — never `default` (the command skill uses `default`; don't mix them up).
- No top-level try/catch — private helpers swallow their own errors and return a safe default.
- `ApiError` is a project-defined type — no `using` needed for it.

## Prerequisites (flag if missing)

If the Application project doesn't yet reference LiteBus or OneOf, add to
`src/TheDeskWatch.Application/TheDeskWatch.Application.csproj`:

```xml
<PackageReference Include="LiteBus.Queries.Abstractions" Version="*" />
<PackageReference Include="OneOf" Version="*" />
```

If `ApiError` isn't defined anywhere in `TheDeskWatch.Application`, tell the user — don't create it
automatically, as its shape is a domain decision.

## After creating the file

Tell the user:
- The path of the file created
- Which TODOs need to be filled in (response properties, implementation body)
- Any missing prerequisites flagged above
