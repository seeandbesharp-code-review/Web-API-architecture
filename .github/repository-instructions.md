# Repository Layer Instructions

## Purpose
The repository layer (UserRepository) encapsulates all data access logic. It must only communicate with the service layer and the EF Core DbContext, never with controllers directly.

## Boundaries
- Repositories **return entities**, not DTOs.
- Services handle DTO mapping, validation, and business rules.
- Do **not** expose `ShopContext` outside repositories.

## Project Structure
```
UserRepository/
  *Repository.cs       # Implementations
  I*Repository.cs      # Interfaces
  ShopContext.cs       # EF Core DbContext (auto-generated)
```

## Required Patterns
- Use **async/await** for all EF Core operations.
- Call `SaveChangesAsync()` after every add/update/delete.
- Avoid new DbContext instances; use DI only.
- Avoid LINQ that executes in memory (no `ToList()` before filtering).

## EF Core Usage Rules
- **DbContext**: `ShopContext` is auto-generated; never edit it.
- **Tracking**: Use `AsNoTracking()` for read-only queries where no update is required.
- **Includes**: Use `Include()` and `ThenInclude()` only when required by the service; keep queries minimal.
- **Find by id**: Prefer `FindAsync()` or `FirstOrDefaultAsync()` with a key predicate.
- **Delete**: Load entity then remove; do not construct a stub entity with only the key.
- **Update**: Attach/Update with the full entity provided by the service (service sets the id).

## Query Conventions
- Use pagination for list endpoints where available (see `PageResponse<T>` in services).
- Always filter in the database, not in memory.
- Keep ordering explicit (e.g., `OrderBy` / `OrderByDescending`).

## Error Handling
- Let EF exceptions bubble to middleware; do not swallow them in repositories.
- Do not log in repositories; logging is handled by middleware and controllers.

## Testing Expectations
- Unit tests use EF In-Memory provider.
- Integration tests use SQL Server LocalDB.
- Repository methods should be deterministic and easily testable.

## Naming
- Interfaces: `IEntityRepository`
- Implementations: `EntityRepository`
- Methods: `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`
