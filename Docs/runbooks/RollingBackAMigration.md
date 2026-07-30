# Runbook: Rolling Back a Migration

## When to use this

You applied a migration locally, then realized it's wrong (wrong column type, forgot a field, bad naming) - before it's been pushed or shared, or before it's touched real data you care about.

## If the migration has NOT been pushed or shared yet

1. Revert the database to the state before this migration:
   ```
   dotnet ef database update <PreviousMigrationName> --startup-project ..\TicketManagement.Api
   ```
   (Use the name of the migration *before* the one you want to undo. To roll back everything, use `0`.)
2. Remove the migration files themselves:
   ```
   dotnet ef migrations remove --startup-project ..\TicketManagement.Api
   ```
3. Fix the underlying Domain/Persistence issue, then generate a fresh migration (see `RunningAMigration.md`).

## If the migration HAS already been pushed or shared

Do **not** delete or rewrite it - once a migration has been shared (pushed, or applied to any database other than your own local one), removing it can desync anyone else's migration history, or your own on a different machine. Instead, write a **new** migration that corrects the issue going forward. Same principle as never rewriting published git history - once it's shared, only add on top of it, never erase it.
