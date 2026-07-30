# Contributing / Git Conventions

This is a solo learning project, but it follows real team conventions deliberately - the practice of using them is the actual point.

## Commit messages - Conventional Commits

Every commit message starts with a type prefix:

- `feat(scope): ...` - a new feature or capability
- `fix(scope): ...` - a bug fix
- `test(scope): ...` - adding or updating tests only
- `chore(scope): ...` - tooling, config, dependency, or housekeeping changes with no behavior change
- `docs(scope): ...` - documentation-only changes
- `refactor(scope): ...` - restructuring code with no behavior change

`scope` is the affected area, lowercase - e.g. `auth`, `tickets`, `authz`.

The first line is a short summary. The body, if needed, explains *why*, not just *what* - the diff already shows what changed; the message should carry the reasoning the diff can't.

## Branching

- `main` is always buildable and deployable. Nothing gets committed to it directly - it's protected against direct pushes.
- Every story gets its own branch: `feature/<short-name>` for new functionality, `fix/<short-name>` for bug fixes, `chore/<short-name>` for tooling/process work.
- Branches are short-lived - opened for one story, merged and deleted once that story's Git Checkpoint is complete. Not kept alive across multiple unrelated stories.

## Merging

- Merge via Pull Request only.
- Default merge strategy: **squash merge**. A feature branch's in-progress commits (typos, "wip", back-and-forth fixes while iterating) collapse into a single clean commit on `main`. The full uncollapsed history is still visible on the closed PR itself, if it's ever needed.

## Pull Requests

- Every PR follows the checklist in `.github/PULL_REQUEST_TEMPLATE.md`.
- Reference the related Issue, if one exists: `Closes #12`.
