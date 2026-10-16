# Decisions

Architecture decision records (ADRs) for MovePlanner. Each entry captures a decision
together with the context it was made in and the consequences that follow from it.

An accepted entry is not edited. When a decision changes, add a new entry and mark the old
one as superseded by it.

## Index

| ID | Title | Status |
| --- | --- | --- |
| [ADR-0001](#adr-0001-multi-project-clean-architecture) | Multi-project clean architecture | Accepted |

Upcoming entries, to be recorded when the corresponding work lands:

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
