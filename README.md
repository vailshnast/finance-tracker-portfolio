# FinanceTracker

[![Build & Test](https://github.com/vailshnast/finance-tracker-portfolio/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/vailshnast/finance-tracker-portfolio/actions/workflows/build-and-test.yml)
[![C#](https://img.shields.io/badge/C%23-13-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![View API Docs](https://img.shields.io/badge/API%20Docs-OpenAPI-85EA2D?logo=swagger&logoColor=black)](https://petstore.swagger.io/?url=https://raw.githubusercontent.com/vailshnast/finance-tracker-portfolio/main/openapi/FinanceTracker.Api.json)

A finance tracking REST API built to go deep on ASP.NET Core architecture — Clean Architecture, CQRS, JWT auth, HybridCache, observability, integration tests. Started from [Mukesh Murugan's Clean Architecture template](https://codewithmukesh.com) as a foundation.

**What I built on top of the template:**
- EF Core `AuditInterceptor` — automatic `CreatedAt`/`UpdatedAt`/`CreatedBy` without polluting handler logic
- `dotnet new` scaffolding templates covering ~70% of feature boilerplate (command, handler, validator, endpoint, tests)
- Integration tests + a specific architecture test that catches missing `FluentValidation` registrations at build time — a real runtime bug I hit and closed permanently
- `Program.cs` split into focused extension methods — 70 lines, no noise
- `Taskfile.yml` for all common commands without `cd` navigation

## Demo (Aspire + Scalar)
![Aspire + Scalar](.github/AspireScalar.gif)


## Tech Stack

| Layer | Technology |
|---|---|
| **Architecture** | Clean Architecture (Domain, Application, Infrastructure, Api) |
| **Runtime** | .NET 10 / C# 13 |
| **API** | Minimal APIs with TypedResults |
| **CQRS** | Manual handlers — zero dependencies, zero licensing risk |
| **Validation** | FluentValidation 12 + Result pattern |
| **Errors** | ProblemDetails (RFC 9457) + global exception handler |
| **Database** | EF Core 10 + PostgreSQL |
| **Caching** | Microsoft HybridCache (L1 in-memory + L2 Redis) |
| **Auth** | ASP.NET Identity + JWT with refresh tokens |
| **API Docs** | Scalar (modern OpenAPI UI) |
| **Logging** | Serilog 10 structured logging |
| **Observability** | .NET Aspire 13 + OpenTelemetry (traces, metrics, logs) |
| **Testing** | xUnit v3 + FluentAssertions + NSubstitute + NetArchTest |
| **Solution** | `.slnx` format + Central Package Management |


## Architecture

```
┌──────────────────────────────────────────────────┐
│                    Api Layer                      │
│         Endpoints, Program.cs, Scalar            │
└──────────────────┬───────────────────────────────┘
                   │ depends on
┌──────────────────▼───────────────────────────────┐
│              Infrastructure Layer                 │
│     EF Core, Identity, JWT, HybridCache          │
└──────────────────┬───────────────────────────────┘
                   │ depends on
┌──────────────────▼───────────────────────────────┐
│              Application Layer                    │
│      CQRS Handlers, Validators, DTOs             │
└──────────────────┬───────────────────────────────┘
                   │ depends on
┌──────────────────▼───────────────────────────────┐
│                Domain Layer                       │
│    Entities, Result/Error types (zero deps)      │
└──────────────────────────────────────────────────┘
```

**Dependency rule:** each layer depends only on the layer below it. Domain has zero external dependencies. Architecture tests enforce this at build time.


## Project Structure

```
├── src/
│   ├── Api/                 Minimal API endpoints, Scalar, middleware
│   ├── Application/         CQRS commands/queries, handlers, validators
│   ├── Domain/              Entities, enums, Result/Error types
│   ├── Infrastructure/      EF Core, Identity, JWT, caching, migrations
│   ├── AppHost/             Aspire orchestration (PostgreSQL + Redis)
│   └── ServiceDefaults/     OpenTelemetry, health checks, resilience
├── tests/
│   ├── Application.UnitTests/    Handler unit tests
│   ├── Integration.Tests/        End-to-end API tests
│   └── Architecture.Tests/       Dependency rule enforcement
├── templates/               dotnet new scaffolding templates
├── Directory.Build.props    .NET 10, nullable enabled, latest C#
├── Directory.Packages.props Central Package Management
├── docker-compose.yml       PostgreSQL + Redis (standalone, no Aspire)
└── Taskfile.yml             Task runner for all common commands
```


## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Run with Aspire (recommended)

```bash
cd src/AppHost
dotnet run
```

This starts everything automatically:

| Service | URL |
|---|---|
| **API + Scalar docs** | https://localhost:7200/scalar/v1 |
| **Aspire Dashboard** | printed in console output on startup |
| **pgAdmin** | printed in console output on startup |
| **RedisInsight** | printed in console output on startup |

### Run without Aspire

```bash
docker compose up -d                          # start PostgreSQL + Redis
dotnet run --project src/Api                  # start API only
```

### Apply Migrations

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

On first run in Development the database is seeded automatically.

**Default admin credentials:**
- Email: `admin@gmail.com`
- Password: `Admin123`


## Running Tests

```bash
dotnet test src/FinanceTracker.slnx                                              # all tests
dotnet test tests/Application.UnitTests                                          # unit tests only
dotnet test tests/Integration.Tests                                              # integration tests only
dotnet test tests/Architecture.Tests                                             # architecture tests only
dotnet test src/FinanceTracker.slnx --filter "FullyQualifiedName~<TestName>"    # single test by name
```


## Task Automation

A [`Taskfile.yml`](Taskfile.yml) is included to automate common workflows via the [Task](https://taskfile.dev) runner. Available commands:

| Command | Description |
|---|---|
| `task start-aspire` | Start API + Postgres + Redis + Aspire dashboard |
| `task run` | Run API only |
| `task build` | Build the solution |
| `task restore` | Restore NuGet packages |
| `task test` | Run all tests |
| `task test-unit` | Run unit tests only |
| `task test-arch` | Run architecture tests only |
| `task test-filter NAME=x` | Run a single test by name |
| `task migrate` | Apply EF Core migrations |
| `task add-migration NAME=x` | Scaffold a new migration |
| `task remove-migration` | Remove the last unapplied migration |
| `task scaffold-all ENTITY=x ...` | Scaffold feature + endpoint + tests |
| `task install-templates` | Install local `dotnet new` templates |


## Key Design Decisions

| Decision | Why |
|---|---|
| **Manual CQRS** over MediatR | Zero licensing risk. MediatR is commercial since v13. |
| **Scalar** over Swagger UI | Modern UI, faster, better DX. Swagger UI is legacy. |
| **HybridCache** over IMemoryCache | Built-in stampede protection, L1+L2, automatic serialization. |
| **Result pattern** over exceptions | Explicit error handling, no hidden control flow, better API contracts. |
| **AuditInterceptor** over handler logic | Audit fields (`CreatedAt`, `UpdatedBy`) belong at the persistence layer — not scattered across commands. |
| **Manual handler registration** over Scrutor | Zero extra dependencies — assembly reflection is ~40 lines. |
| **`.slnx`** over `.sln` | XML-based, merge-friendly, the future of .NET solutions. |


## AI Development with CLAUDE.md

This project includes a [`CLAUDE.md`](CLAUDE.md) file — a markdown file that [Claude Code](https://claude.ai/code) reads automatically at the start of every session. It gives the AI persistent, project-specific context that cannot be inferred from the code alone.

### What it solves

Without it, every new Claude session starts cold — you re-explain the same conventions, correct the same wrong suggestions, and fight the same bad defaults. CLAUDE.md eliminates that repetition by encoding project rules once, permanently.

### What this project's CLAUDE.md contains

| Section | Purpose |
|---|---|
| **Tech Stack** | Exact versions and key libraries so Claude targets the right APIs |
| **Architecture Rules** | No repository pattern, no AutoMapper, handlers must be sealed — prevents wrong suggestions |
| **Naming Conventions** | `Create[Entity]Command`, `Get[Entity]Query`, `[Command]Handler` — keeps generated code consistent |
| **Patterns to Avoid** | MediatR, Scrutor, stored procedures, exceptions for business logic — Claude will never suggest these |
| **Commands** | All `task` and `dotnet` commands with their raw equivalents |
| **Domain Terms** | What `Category`, `Transaction`, `Budget` map to — prevents misinterpretation |

### Design principle

Every line in CLAUDE.md loads into every conversation and consumes context. The file is intentionally lean — only rules that would cause a real mistake if missing. Obvious practices are left out; only the non-obvious constraints specific to this codebase are encoded.

**Further reading:**
- [Claude Code Tips & CLAUDE.md Best Practices](https://codewithmukesh.com/blog/claude-code-tips-advanced/)
- [CLAUDE.md Mastery for .NET](https://codewithmukesh.com/blog/claude-md-mastery-dotnet/)
- [Using CLAUDE.md Files](https://claude.ai/blog/using-claude-md-files)


## About

Built by **Valentyn Matvieienko** — 8+ years building production systems in Unity (tooling, XR, CI/CD, backend integrations), now going deep on ASP.NET Core.

- [LinkedIn](https://linkedin.com/in/valentyn-matveenko)
- xanaramus@gmail.com

---

## License

MIT License
