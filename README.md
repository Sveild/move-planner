# MovePlanner

A web app for planning an international move. It is built from modules, starting with a job application tracker for finding work in the destination country.

## Tech stack

**Frontend**

- Angular (planned; the client has not been added yet)

**Backend**

- .NET 10 / ASP.NET Core
- Wolverine for handler discovery, the handler middleware pipeline, HTTP endpoints and the
  PostgreSQL-backed outbox
- FluentValidation for request validation (planned; the validation issue is in the current milestone)
- PostgreSQL through Entity Framework Core
- xUnit v3 on Microsoft.Testing.Platform, with Testcontainers for the tests that need PostgreSQL and
  Wolverine's tracked sessions for the tests that wait on handlers

## Prerequisites

- .NET 10 SDK
- Docker, for the local database and for the tests that use Testcontainers
- Node.js and the Angular CLI, once the client is added

## Architecture

The solution follows a layered clean architecture, recorded as
[ADR-0001](decisions.md#adr-0001-multi-project-clean-architecture). A project may only reference the
projects below it:

- `src/MovePlanner.Domain` — entities, value objects and domain rules. References nothing.
- `src/MovePlanner.Application` — use cases. References Domain.
- `src/MovePlanner.Infrastructure` — persistence and other adapters. References Application.
- `src/MovePlanner.Api` — HTTP entry point and composition root. References Application and
  Infrastructure.

### Requests

A request travels through Wolverine. An endpoint in `src/MovePlanner.Api` binds the `Application`
command or query directly, hands it to the message bus and returns the result; the handler in
`src/MovePlanner.Application` runs inside Wolverine's middleware pipeline, which opens the database
transaction and flushes the outbox around it. Endpoints hold no business logic — they bind, dispatch
and shape the response. The `[WolverineGet]`, `[WolverinePost]`, `[WolverinePut]` and
`[WolverineDelete]` attributes carry the routes, and `MapWolverineEndpoints()` is the only mapping
([ADR-0005](decisions.md#adr-0005-http-endpoints-are-thin-adapters-over-the-message-bus)).

Commands, queries and DTOs are immutable records and form the public API contract. They use
primitives and never expose domain entities or value objects
([ADR-0002](decisions.md#adr-0002-adopt-wolverine-as-the-mediator-handler-pipeline-and-messaging-layer)).
Handlers return the response value or nothing and signal expected failures with typed exceptions,
which `GlobalExceptionHandler` in `Api` turns into ProblemDetails responses
([ADR-0003](decisions.md#adr-0003-adopt-exception-based-error-handling)).

Validation lives in `Application` next to the command or query, as FluentValidation validators that
run in the handler pipeline, so HTTP and non-HTTP callers share one path. It is not added yet.

Domain events go through the transactional outbox: Wolverine's Entity Framework Core middleware
writes outgoing messages in the same transaction as the entity changes, and a background dispatcher
delivers them afterwards. The outbox is provisioned, but no domain events are published yet.

### Storage

Entity Framework Core migrations own the application tables. Wolverine owns its message storage in a
separate `wolverine` schema, and creates or updates it on startup in Development only. Every other
environment is provisioned explicitly by the deployment and fails at startup when the storage is
missing ([ADR-0004](decisions.md#adr-0004-wolverine-owns-its-message-storage-schema)).

The tests live in `tests`:

- `MovePlanner.Domain.Tests`, `MovePlanner.Application.Tests` and `MovePlanner.Infrastructure.Tests`
  hold the unit tests of each layer. Handlers are called as plain methods, with fakes for their
  dependencies.
- `MovePlanner.Architecture.Tests` fails the build when a dependency points the wrong way or a
  Wolverine convention is broken (where handlers, endpoints and validators live, immutable commands
  and queries, exceptions instead of result wrappers).
- `MovePlanner.Api.IntegrationTests` boots the real application with `WebApplicationFactory` against
  a PostgreSQL container, so Wolverine discovers the `Application` assembly and provisions its schema
  itself. Tests of asynchronous flows use Wolverine's tracked sessions (`TrackActivity`) to wait for
  the handlers instead of sleeping.
- `MovePlanner.Tests.Support` holds the shared fixtures, such as the PostgreSQL container and the
  isolated database a test class can own.

## Development

Build, verify formatting and run the tests:

```sh
dotnet build MovePlanner.slnx -warnaserror
dotnet format MovePlanner.slnx --verify-no-changes
dotnet test MovePlanner.slnx
```

`dotnet test` starts PostgreSQL containers through Testcontainers, so Docker has to be running.
Collect a coverage report with:

```sh
dotnet test MovePlanner.slnx --coverage --coverage-output-format cobertura
```

Start the API; it needs a configured connection string and a running database, described below:

```sh
dotnet run --project src/MovePlanner.Api
```

In Development the API serves the OpenAPI document at `/openapi/v1.json` and a Scalar UI at
`/scalar/v1`. The document can also be generated without starting the API, which is how a build or a
deployment captures it:

```sh
dotnet run --project src/MovePlanner.Api -- openapi
```

The analyzer and suppression rules are in [CONTRIBUTING.md](CONTRIBUTING.md), and the conventions for
automated agents are in [AGENTS.md](AGENTS.md).

## Configuration

The API reads settings from `appsettings.json`, `appsettings.{Environment}.json`, environment
variables and, in development, user-secrets. `appsettings.json` holds only non-sensitive defaults;
secrets such as the database connection string are never committed.

For local development, store the connection string in user-secrets:

```sh
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=moveplanner;Username=moveplanner;Password=moveplanner" --project src/MovePlanner.Api
```

Required settings are validated when the application starts, so a missing or invalid connection
string fails immediately instead of at first use.

## Database

The API uses PostgreSQL through Entity Framework Core. Migrations live in
`src/MovePlanner.Infrastructure`, with the API project as the startup project.

Start a local database with Docker. The credentials come from an untracked `.env` file, so copy the
committed example before the first run:

```sh
cp .env.example .env
docker compose up -d
docker compose down
```

pgAdmin comes up alongside the database at http://localhost:5050, using the email and password from
`.env`. Register the server with host `postgres`, port `5432` and the PostgreSQL credentials from the
same file.

The Entity Framework Core tools read the connection string from the `Database__ConnectionString`
environment variable. Apply the migrations and list them:

```sh
Database__ConnectionString="Host=localhost;Port=5432;Database=moveplanner;Username=moveplanner;Password=moveplanner" \
  dotnet ef database update --project src/MovePlanner.Infrastructure --startup-project src/MovePlanner.Api
Database__ConnectionString="Host=localhost;Port=5432;Database=moveplanner;Username=moveplanner;Password=moveplanner" \
  dotnet ef migrations list --project src/MovePlanner.Infrastructure --startup-project src/MovePlanner.Api
```

The migrations cover the application tables only. Wolverine keeps its message storage in the
`wolverine` schema, which Development creates or updates when the API starts
([ADR-0004](decisions.md#adr-0004-wolverine-owns-its-message-storage-schema)). A deployment runs that
setup step explicitly instead:

```sh
Database__ConnectionString="Host=localhost;Port=5432;Database=moveplanner;Username=moveplanner;Password=moveplanner" \
  dotnet run --project src/MovePlanner.Api -- resources setup
```

## Decisions

Architecture decisions are recorded in [decisions.md](decisions.md), each with its context,
decision and consequences:

- [ADR-0001](decisions.md#adr-0001-multi-project-clean-architecture) — multi-project clean
  architecture
- [ADR-0002](decisions.md#adr-0002-adopt-wolverine-as-the-mediator-handler-pipeline-and-messaging-layer)
  — Wolverine as the mediator, handler pipeline and messaging layer
- [ADR-0003](decisions.md#adr-0003-adopt-exception-based-error-handling) — exception-based error
  handling
- [ADR-0004](decisions.md#adr-0004-wolverine-owns-its-message-storage-schema) — Wolverine owns its
  message storage schema
- [ADR-0005](decisions.md#adr-0005-http-endpoints-are-thin-adapters-over-the-message-bus) — HTTP
  endpoints are thin adapters over the message bus
- [ADR-0006](decisions.md#adr-0006-openapi-documents-the-endpoints-and-scalar-renders-it-in-development)
  — OpenAPI documents the endpoints and Scalar renders it in Development
