# MovePlanner

A web app for planning an international move. It is built from modules, starting with a job application tracker for finding work in the destination country.

## Tech stack

**Frontend**

- Angular

**Backend**

- ASP.NET Core

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

## Decisions

Architecture decisions are recorded in [decisions.md](decisions.md).
