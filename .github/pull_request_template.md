## Summary

<!-- What does this PR do, and why? Keep it short. -->

Closes #

## Type of change

- [ ] Feature
- [ ] Bug fix
- [ ] Refactor (no behaviour change)
- [ ] Infrastructure / CI / tooling
- [ ] Documentation

## Area

- [ ] Domain
- [ ] Application
- [ ] Infrastructure
- [ ] Api
- [ ] Client (Angular)
- [ ] Tests only

Module: <!-- e.g. job applications -->

## Changes

<!-- Main changes, one bullet each. Mention anything reviewers should look at first. -->

-

## How to test

<!-- Steps or commands to verify the change. Include example requests for new endpoints. -->

## Checklist

**General**

- [ ] The PR is linked to an issue
- [ ] The change stays inside the right layer and module (see the README architecture section)
- [ ] New or changed behaviour is covered by tests
- [ ] No secrets, keys or `.env` files are committed

**API**

- [ ] `dotnet build -warnaserror` passes
- [ ] `dotnet format --verify-no-changes` passes
- [ ] `dotnet test` passes (including architecture tests)
- [ ] No analyzer warnings were suppressed, or each suppression has a justification
- [ ] Database changes include a migration, and the migration was applied locally
- [ ] New endpoints appear correctly in the OpenAPI docs

**Client**

- [ ] `npx prettier --check .` passes
- [ ] `npm test` and `npm run build` pass
- [ ] Checked in light and dark mode (if the UI changed)

**Docs**

- [ ] README or `decisions.md` updated if behaviour, setup or architecture changed

## Screenshots

<!-- For UI changes: before and after. Delete this section otherwise. -->

## Notes for reviewers

<!-- Trade-offs, follow-ups, known limitations. -->
