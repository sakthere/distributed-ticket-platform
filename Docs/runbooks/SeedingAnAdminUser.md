# Runbook: Creating the First Admin User (Local Dev Only)

## Why this exists

`Register` only ever creates `Employee` accounts, by design - see `Docs/revision/Sprint3_RoleBasedAuthorization.md` for the full reasoning (self-registration must never be able to grant Admin access). There is currently no in-app way to create an Admin account. This is tracked as intentional tech debt; a proper seeding/bootstrap mechanism is listed as a Future Improvement in that same recap.

## Steps

1. Register a normal account through the API as usual.
2. Connect to the local dev database (SQL Server Management Studio, or your preferred SQL client).
3. Run:
   ```sql
   UPDATE Users SET Role = 3 WHERE Email = 'your-test-admin@example.com';
   ```
   (`3` = `Admin`, per `TicketManagement.Domain.Enums.UserRole`.)
4. Log in again (or refresh) to get a new access token carrying the updated role - an already-issued token still carries the old role until it expires, per the stateless-JWT tradeoff discussed in Sprint 3.

## Do not do this in any real or shared environment

This is a local-dev-only workaround. A real environment needs an actual seeding mechanism - never run a manual role-elevation query against a shared or production database.
