# Agent notes

Conventions for automated agents working in this repository.

## Assignment

- Assign every issue and pull request to `Sveild`.

## Pull requests

- Branch name: `<issue-number>-<slug>` (for example `24-application-dependency-injection`), created from the latest `origin/main`.
- Exactly one commit per pull request. CI enforces this, so amend and force-push instead of adding commits.
- Commit message: a conventional prefix (`feat:`, `fix:`, `chore:`, `ci:`, `docs:`, `test:`), a subject, a blank line, `-` bullets, then `Refs #<issue>`.
- Fill in `.github/pull_request_template.md` when opening the pull request.

## Commands

- Build: `dotnet build MovePlanner.slnx -warnaserror`
- Format: `dotnet format MovePlanner.slnx --verify-no-changes`
- Test: `dotnet test MovePlanner.slnx`
- Coverage (locally): `dotnet test MovePlanner.slnx --coverage --coverage-output-format cobertura`
- Use the `rtk git` / `rtk gh` wrappers. Pass the solution path directly to `dotnet test` (the `rtk dotnet` wrapper rejects positional arguments).

## Notes

- Analyzers are strict (`AnalysisMode=All`, `TreatWarningsAsErrors`). Prefer fixing over suppressing; any suppression needs a justification in `.editorconfig`.
- `dotnet test` needs Docker running for the infrastructure and integration test projects (Testcontainers).
