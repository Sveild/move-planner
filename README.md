# MovePlanner

A web app for planning an international move. It is built from modules, starting with a job application tracker for finding work in the destination country.

## Tech stack

**Frontend**

- Angular (planned; the client has not been added yet)

**Backend**

- .NET 10 / ASP.NET Core
- PostgreSQL through Entity Framework Core
- xUnit v3 on Microsoft.Testing.Platform, with Testcontainers for the infrastructure and integration tests

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

The tests live in `tests`:

- `MovePlanner.Domain.Tests`, `MovePlanner.Application.Tests` and `MovePlanner.Infrastructure.Tests`
  hold the unit tests of each layer.
- `MovePlanner.Architecture.Tests` fails the build when a dependency points the wrong way.
- `MovePlanner.Api.IntegrationTests` exercises the API end to end.
- `MovePlanner.Tests.Support` holds shared fixtures, such as the PostgreSQL container used by the
  infrastructure and integration tests.

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
`src/MovePlanner.Infrastructure`, with the API project as the startup project. The Entity Framework
Core tools read the connection string from the `Database__ConnectionString` environment variable.

```sh
Database__ConnectionString="Host=localhost;Port=5432;Database=moveplanner;Username=moveplanner;Password=moveplanner" \
  dotnet ef migrations list --project src/MovePlanner.Infrastructure --startup-project src/MovePlanner.Api
```

## Decisions

Architecture decisions are recorded in [decisions.md](decisions.md).
