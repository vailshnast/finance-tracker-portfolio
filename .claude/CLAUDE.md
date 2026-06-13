# CLAUDE.md - FinanceTracker

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview
A personal finance tracking API for managing transactions, categories, and budgets with JWT authentication.

## Tech Stack
- .NET 10, ASP.NET Core Minimal APIs
- Entity Framework Core 10 with PostgreSQL (Aspire-managed)
- Custom CQRS abstractions (`ICommand<T>`, `IQuery<T>`, `ICommandHandler`, `IQueryHandler`) — no Mediator library
- FluentValidation for request validation (runs via `ValidationFilter<T>` endpoint filter)
- HybridCache (Redis L2 + in-process L1) via .NET Aspire
- ASP.NET Core Identity + JWT Bearer with refresh tokens
- Scalar for OpenAPI docs (`/scalar/v1`)
- Serilog for structured logging + OpenTelemetry
- .NET Aspire for orchestration (Postgres + pgAdmin, Redis + RedisInsight)
- xUnit + FluentAssertions + NSubstitute for testing
- NetArchTest for architecture boundary enforcement

## Project Structure
- `src/Api/` - Minimal API endpoints, DI wiring, exception handler, OpenAPI config
- `src/Application/` - CQRS interfaces, commands, queries, handlers, validators, feature use cases
- `src/Domain/` - Entities, enums, Result/Error types — zero external dependencies
- `src/Infrastructure/` - EF Core DbContext, JWT TokenService, CurrentUser, migrations
- `src/AppHost/` - .NET Aspire orchestration (Postgres, Redis, API)
- `src/ServiceDefaults/` - OpenTelemetry, health checks, resilience HTTP client
- `tests/Application.UnitTests/` - Handler and domain logic tests (EF InMemory + NSubstitute)
- `tests/Architecture.Tests/` - Dependency boundary + sealed-type enforcement (NetArchTest)

## Commands
Common workflows are automated in [`Taskfile.yml`](Taskfile.yml) via the [Task](https://taskfile.dev) runner. Raw equivalents shown where relevant.

**Run:**
```
task start-aspire                    # API + Postgres + Redis + Aspire dashboard
task run                             # API only → dotnet run --project src/Api
task build                           # dotnet build src/FinanceTracker.slnx
```

**Test:**
```
task test                            # dotnet test src/FinanceTracker.slnx
task test-unit                       # dotnet test tests/Application.UnitTests
task test-arch                       # dotnet test tests/Architecture.Tests
task test-filter NAME=<TestName>     # dotnet test ... --filter "FullyQualifiedName~<Name>"
```

**Database:**
```
task migrate                         # dotnet ef database update ...
task add-migration NAME=<Name>       # dotnet ef migrations add <Name> ...
task remove-migration                # dotnet ef migrations remove ...
task list-migrations                 # dotnet ef migrations list ...
```

**Scaffolding:**
```
task scaffold-all ENTITY=<Entity> DB_SET=<DbSet> ROUTE=<route>
task install-templates               # install local dotnet new templates
```

## Architecture Rules
- Domain layer has ZERO external dependencies
- Application layer defines interfaces (`IAppDbContext`, `ICurrentUser`, `ITokenService`); Infrastructure implements them
- All database access goes through `IAppDbContext` directly — no repository pattern
- API layer is thin — endpoint definitions only, delegates to handlers via DI
- All handlers and validators **must be sealed** (enforced by `Architecture.Tests`)
- Package versions are centralized in `Directory.Packages.props` — never add versions in individual `.csproj` files

## Code Conventions

### Naming
- Commands: `Create[Entity]Command`, `Update[Entity]Command`, `Delete[Entity]Command`
- Queries: `Get[Entity]Query`, `GetAll[Entity]Query`
- Handlers: `[Command/Query]Handler` — sealed, co-located with the command/query
- Validators: `[Command/Query]Validator` — sealed, co-located with the command/query
- Endpoints: `[Entity]Endpoints` mapping to `/api/[entity]`
- Responses: `[Entity]Response` as records

### Patterns We Use
- Primary constructors for DI
- Records for commands, queries, and response DTOs
- `Result<T>` / `Result` for error handling — no exceptions for business logic
- File-scoped namespaces
- Always pass `CancellationToken` to async methods
- Feature folders: `Application/Features/<Domain>/<Operation>/` mirrored in `tests/Application.UnitTests/Features/`
- New entities: add `DbSet<T>` to `IAppDbContext`, EF config as `IEntityTypeConfiguration<T>` in Infrastructure

### Patterns We DON'T Use (Never Suggest)
- Repository pattern (use `IAppDbContext` / EF Core directly)
- AutoMapper (write explicit mappings)
- Exceptions for business logic errors (use `Result` / `Error`)
- Mediator library (the project has its own lightweight CQRS abstractions)
- Stored procedures

## Validation
- All request validation in sealed FluentValidation validators co-located with the command/query
- Validators auto-registered via assembly scanning in `AddApplication()`
- Validation runs via `ValidationFilter<T>` endpoint filter before handler invocation
- Invalid requests return HTTP 400 `ValidationProblem` with per-field error groups

## Error Handling
- `Error` records carry a code, description, and type: `NotFound`, `Validation`, `Conflict`, `Failure`
- API layer maps `Result` to `ProblemDetails` with the corresponding HTTP status code
- `GlobalExceptionHandler` catches unhandled exceptions, logs them, and returns HTTP 500

## Testing
- Unit tests: handler logic with NSubstitute mocks and EF Core InMemory database
- Architecture tests: verify Clean Architecture dependency rules and sealed constraints — run before committing new layers or types
- Use FluentAssertions for assertions
- Test naming: `[Method]_[Scenario]_[ExpectedResult]`

## Git Workflow
- Branch naming: `feature/`, `bugfix/`, `hotfix/`
- Commit format: `type: description` (feat, fix, refactor, test, docs)
- Always create a branch before making changes
- Run tests before committing

## Domain Terms
- `Category` — income or expense bucket (CategoryType enum: Income / Expense); linked to Transactions and Budgets
- `Transaction` — a single financial movement with amount, description, date; linked to a Category and ApplicationUser
- `Budget` — a monthly spending limit for a specific Category, scoped to an ApplicationUser
- `ApplicationUser` — extends ASP.NET Identity `IdentityUser` with FirstName, LastName, and RefreshToken
