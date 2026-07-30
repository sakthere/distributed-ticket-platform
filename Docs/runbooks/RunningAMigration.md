# Runbook: Adding and Applying an EF Core Migration

## When to use this

Any time a Domain entity's shape changes (new property, new relationship, renamed column) and that change needs to reach the actual database.

## Steps

1. Make your Domain/Persistence changes first (entity properties, `IEntityTypeConfiguration` changes) and confirm the project builds.
2. From inside `TicketManagement.Persistence`, generate the migration:
   ```
   dotnet ef migrations add <DescriptiveName> --startup-project ..\TicketManagement.Api
   ```
   `--startup-project ..\TicketManagement.Api` is required - EF Core needs the API project's configuration (connection string, DI setup) to generate the migration, even though the migration files themselves live in Persistence. Forgetting the `..\` (running from the wrong working directory, or omitting the relative path) is the single most common mistake here - it fails with a confusing `IndexOutOfRangeException` or `MSB1009: project file does not exist`, neither of which obviously points at the real cause.
3. **Read the generated migration file before applying it.** Confirm it only contains the changes you expected - EF Core occasionally infers something you didn't intend (an unwanted cascade delete, a column that already looked correct getting touched anyway).
4. Apply it to your local database:
   ```
   dotnet ef database update --startup-project ..\TicketManagement.Api
   ```
5. Confirm the app still starts and the affected feature still works end-to-end before committing.

## Common errors

- `IndexOutOfRangeException` / `MSB1009: project file does not exist` - almost always the missing `--startup-project ..\` path issue above.
