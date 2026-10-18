# Decisions

Architecture decision records (ADRs) for MovePlanner. Each entry captures a decision
together with the context it was made in and the consequences that follow from it.

An accepted entry is not edited. When a decision changes, add a new entry and mark the old
one as superseded by it.

## Index

| ID | Title | Status |
| --- | --- | --- |
| [ADR-0001](#adr-0001-multi-project-clean-architecture) | Multi-project clean architecture | Accepted |
| [ADR-0002](#adr-0002-adopt-wolverine-as-the-mediator-handler-pipeline-and-messaging-layer) | Adopt Wolverine as the mediator, handler pipeline and messaging layer | Accepted |

Upcoming entries, to be recorded when the corresponding work lands:

- Error handling (exception-based).
- Feature modules living inside the layers.
- Test runner: xUnit v3 on Microsoft.Testing.Platform.
- Analyzers that fail the build.
- Central package management.

## Template

Copy the block below, give the entry the next sequential ID and add it to the index.

```markdown
## ADR-XXXX: Title

- **Status:** Accepted
- **Date:** YYYY-MM-DD

### Context

What is the problem, and which forces and constraints are in play?

### Decision

What we decided to do.

### Consequences

What becomes easier, and what becomes harder or is accepted as a cost.
```

## ADR-0001: Multi-project clean architecture

- **Status:** Accepted
- **Date:** 2026-10-17

### Context

The application is expected to grow, and keeping every layer in a single project makes it
easy to reach across a boundary. Those shortcuts are hard to notice in review, and once
taken they tend to spread. This layout is also the one used in other projects, so it is
familiar and quick to apply.

### Decision

Split the code into four projects, each only able to reference the ones below it:

- `MovePlanner.Domain` — entities, value objects and domain rules. References nothing.
- `MovePlanner.Application` — use cases. References Domain.
- `MovePlanner.Infrastructure` — persistence and other adapters. References Application.
- `MovePlanner.Api` — HTTP entry point and composition root. References Application and
  Infrastructure.

### Consequences

- The separation is easier to keep: a wrong reference is a compile error, and the
  architecture tests fail the build when a dependency points the wrong way.
- Concerns stay clearly separated, with less temptation to break the boundaries.
- Accepted cost: mapping between HTTP and application types adds code.

## ADR-0002: Adopt Wolverine as the mediator, handler pipeline and messaging layer

- **Status:** Accepted
- **Date:** 2026-10-18

### Context

The application needs a way to dispatch commands and queries from HTTP endpoints to use cases, and
later a way to publish domain events reliably, without hand-rolling that plumbing. The current
pieces — the `IHandler<TCommand, TResponse>` contract, handler auto-registration and the handler
endpoint extensions — work, but they are bespoke, and extending them for events and an outbox would
mean building a home-made mediator, background dispatcher and outbox.

MediatR, the obvious mediator, is licensed commercially in its newer versions. Wolverine covers the
same ground under a single dependency: handlers, a middleware pipeline, HTTP endpoints and a
transactional outbox backed by PostgreSQL. That lets the bespoke pieces be deleted instead of
extended.

### Decision

Adopt Wolverine as the mediator, handler pipeline and messaging library. The four-project layout
from ADR-0001 is unchanged.

- Handlers are plain public classes in `Application`, found by Wolverine's discovery conventions.
  Commands and queries are plain records. There is no custom `IHandler` contract and no assembly
  scanning.
- `Wolverine.HTTP` endpoints live in `Api` and stay thin: they bind the `Application` command or
  query directly, call `IMessageBus.InvokeAsync` and return the result. There are no Api request
  DTOs and no `ToCommand`/`ToQuery` mapping, as in the Clean Architecture template. A route id is an
  endpoint parameter merged into the command (for example a command with `Id = id`). Endpoints hold
  no business logic.
- Commands and queries are the public API contract: immutable records using primitives (`Guid` ids,
  strings, numbers, dates), Domain enums serialized as strings, and DTO records for responses. They
  never expose domain entities or value objects such as `ApplicationId`; the handlers convert.
  Changing a command or query is an API change.
- FluentValidation validators live in `Application` next to the commands and queries and run in
  Wolverine's handler middleware, so HTTP and non-HTTP callers share one validation path.
  `ValidationException` is turned into a 400 ProblemDetails response by the global exception handler,
  and `WolverineFx.Http.FluentValidation` is not used, to avoid validating twice. The rest of the
  error-handling strategy is recorded separately.
- `Application` may reference the core `WolverineFx` package (for example `IMessageBus` or
  `OutgoingMessages`) and FluentValidation, but never `WolverineFx.Http` or ASP.NET Core. `Domain`
  references no Wolverine types.
- `Infrastructure` owns the persistence wiring: EF Core transactions, PostgreSQL message storage and
  the outbox.
- Wolverine scans only the application assembly for handlers by default, so the `Application`
  assembly is included explicitly.
- The licensing terms of the Wolverine packages in use are confirmed before any commercial feature is
  relied on.

Alternatives considered:

- **Api request DTOs with mapping** (`ToCommand`/`ToQuery`): rejected. They duplicate types for a
  single HTTP client and add a mapping step with no consumer to justify it.
- **Endpoint-as-handler** (Wolverine.HTTP endpoints as the handlers): rejected. It moves use cases
  into `Api` and prevents reuse by message consumers.

Superseded by this decision: the custom `IHandler` contract, handler auto-registration, the handler
endpoint extensions, Api request DTOs with `ToCommand`/`ToQuery` mapping, the custom outbox table,
interceptor and processor, and custom retry and dead-letter handling.

### Consequences

- One dependency replaces several hand-rolled pieces, so there is less code to own and test.
- Use cases can later be triggered from messages (queues, scheduled jobs) without going through HTTP,
  and the transactional outbox comes with the library.
- Accepted cost: application code now follows Wolverine's conventions, so handler and endpoint styles
  are Wolverine's rather than fully custom.
- Accepted cost: a third-party dependency with its own licensing terms and release cadence to follow.

