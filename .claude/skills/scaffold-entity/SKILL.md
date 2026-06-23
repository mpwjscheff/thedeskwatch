---
name: scaffold-entity
description: Scaffold a new domain entity and its complete persistence stack in TheDeskWatch. Creates the domain model, EF Core fluent configuration, DbContext DbSet, repository interface + implementation, and DI wiring in one step. Use this skill whenever the user wants to add a new entity, create a new database table, add a model to the database, or store a new concept persistently — e.g. "add a Task entity", "create a Note model", "I need to store X in the database", "scaffold X persistence", "add X to the DB". Always use this skill for any domain entity creation — do not write these files manually without it.
---

## Overview

This skill creates the full persistence stack for one new domain entity across three projects:

| File | Project |
|------|---------|
| `TheDeskWatch.Domain/{EntityName}.cs` | Domain — pure model, no framework deps |
| `TheDeskWatch.Persistence/Data/Configurations/{EntityName}Configuration.cs` | Persistence — EF Core fluent mapping |
| `TheDeskWatch.Persistence/Data/DeskWatchDbContext.cs` *(patched)* | Persistence — add `DbSet<T>` |
| `TheDeskWatch.Persistence/Repositories/I{EntityName}Repository.cs` | Persistence — public interface |
| `TheDeskWatch.Persistence/Repositories/{EntityName}Repository.cs` | Persistence — internal implementation |
| `TheDeskWatch.Persistence/ServiceCollectionExtensions.cs` *(patched)* | Persistence — DI registration |

Finish by running `dotnet build TheDeskWatch.slnx` to confirm everything compiles cleanly.

## Non-negotiable conventions

These are enforced as build errors — getting them wrong will fail `dotnet build`:

1. **Using directives go OUTSIDE (before) the `namespace` block** — IDE0065 is a compile error in this repo; every `using` must appear above the `namespace` line.
2. **Sealed classes with primary constructors** — prefer `public sealed class Foo(Bar dep)` over constructor bodies with field assignments.
3. **Nullable reference types** — every reference-type property needs `required`, `?`, or a non-null default. Missing nullability annotations are warnings-as-errors.
4. **No EF Core attributes on domain entities** — use only the fluent API in the configuration class. The Domain project has zero NuGet dependencies and must stay that way.

## Step 1 — Gather inputs

Read the user's request carefully. You need:

- **Entity name** — singular PascalCase (`Task`, `Note`, `TimerEntry`)
- **Properties** — name, CLR type, and nullability for each one

If the prompt already contains this, proceed immediately. If anything is ambiguous, ask one short question. Do not ask for what you can infer.

## Step 2 — Domain entity

**Create**: `src/TheDeskWatch.Domain/{EntityName}.cs`

```csharp
namespace TheDeskWatch.Domain;

public sealed class {EntityName}
{
    public int Id { get; set; }

    // ... user-specified properties ...
}
```

Rules:
- `Id` is always `int` and serves as the primary key (configured in the EF mapping, not here).
- Reference-type properties use `required` when they must always be set, or `string?` / `T?` when optional.
- No constructors — EF Core materializes entities through property setters.
- No framework or package references — the Domain project is a pure .NET library.

## Step 3 — EF Core configuration

**Create**: `src/TheDeskWatch.Persistence/Data/Configurations/{EntityName}Configuration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheDeskWatch.Domain;

namespace TheDeskWatch.Persistence.Data.Configurations;

internal sealed class {EntityName}Configuration : IEntityTypeConfiguration<{EntityName}>
{
    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(e => e.Id);
        // Add constraints that reflect the domain rules: IsRequired(), HasMaxLength(), HasIndex(), etc.
    }
}
```

`DeskWatchDbContext.OnModelCreating` calls `modelBuilder.ApplyConfigurationsFromAssembly(...)`, so this class is **discovered automatically** — no manual registration needed.

## Step 4 — DbContext DbSet

**Edit**: `src/TheDeskWatch.Persistence/Data/DeskWatchDbContext.cs`

Add one property inside the class body, after the existing `DbSet<>` properties (or as the first one if none exist):

```csharp
public DbSet<{EntityName}> {EntityNamePlural} { get; set; }
```

Keep every other line untouched.

## Step 5 — Repository interface

**Create**: `src/TheDeskWatch.Persistence/Repositories/I{EntityName}Repository.cs`

```csharp
using TheDeskWatch.Domain;

namespace TheDeskWatch.Persistence.Repositories;

public interface I{EntityName}Repository
{
    Task<{EntityName}?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<{EntityName}>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync({EntityName} entity, CancellationToken cancellationToken = default);
    Task UpdateAsync({EntityName} entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
```

## Step 6 — Repository implementation

**Create**: `src/TheDeskWatch.Persistence/Repositories/{EntityName}Repository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using TheDeskWatch.Domain;
using TheDeskWatch.Persistence.Data;

namespace TheDeskWatch.Persistence.Repositories;

internal sealed class {EntityName}Repository(DeskWatchDbContext context) : I{EntityName}Repository
{
    public Task<{EntityName}?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => context.{EntityNamePlural}.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<{EntityName}>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.{EntityNamePlural}.ToListAsync(cancellationToken);

    public async Task AddAsync({EntityName} entity, CancellationToken cancellationToken = default)
    {
        context.{EntityNamePlural}.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync({EntityName} entity, CancellationToken cancellationToken = default)
    {
        context.{EntityNamePlural}.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is not null)
        {
            context.{EntityNamePlural}.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

## Step 7 — DI registration

**Edit**: `src/TheDeskWatch.Persistence/ServiceCollectionExtensions.cs`

Add a scoped registration inside `AddPersistence`, after `AddDbContext`:

```csharp
services.AddScoped<I{EntityName}Repository, {EntityName}Repository>();
```

You will also need to add a `using TheDeskWatch.Persistence.Repositories;` directive if it is not already present. Remember: the `using` goes **above** the `namespace` line.

## Step 8 — Build verification

```bash
dotnet build TheDeskWatch.slnx
```

If the build fails, read the diagnostic carefully and fix the root cause. Never suppress a warning — resolve it.

## Scope boundary

This skill stops at the persistence boundary. Do **not** scaffold Application layer commands, queries, or services — those are handled by the `scaffold-command` and `scaffold-query` skills.
