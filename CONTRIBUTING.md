# Contributing

## Code quality and analyzers

The build treats warnings as errors. A full set of analyzers (built-in .NET/CA rules,
SonarAnalyzer, Meziantou.Analyzer, Roslynator and the Banned API analyzer) runs on every
project, so a new warning fails the build.

Rules:

- **Prefer fixing the code over suppressing the diagnostic.** Reach for a suppression only
  when the warning is a genuine false positive or the rule does not fit the context.
- **Scope suppressions in `.editorconfig`, not in code.** A severity override next to the
  other rules is easier to review than scattered inline comments. Add a short comment
  explaining *why* the rule is disabled, next to the line.
- **Any `#pragma warning disable` or `[SuppressMessage]` needs a justification.** State the
  reason (and an issue link if relevant) on or directly above the suppression. An
  unexplained suppression will be asked about in review.
- **Banned APIs.** `DateTime.Now`/`UtcNow` and `DateTimeOffset.Now`/`UtcNow` are banned;
  inject `TimeProvider` instead. `Task.Wait`, `Task.Result` and `Thread.Sleep` are also
  banned in favour of awaiting and `Task.Delay`. The ban list lives in `BannedSymbols.txt`.

Before opening a pull request, make sure the local checks pass:

```
dotnet build -warnaserror
dotnet format --verify-no-changes
dotnet test
```
