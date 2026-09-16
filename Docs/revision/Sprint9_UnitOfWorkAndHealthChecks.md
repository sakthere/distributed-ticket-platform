# Sprint 9 - Unit of Work and Health Checks

## Milestone

Phase 3 (Engineering Improvements) begins. This sprint covers the first two items: making the commit boundary explicit across the whole Application layer, and giving the API a real way for a load balancer/orchestrator to ask "is this instance okay?"

```
Before                                   After
-----------------------------            -----------------------------
_ticketRepository.SaveChangesAsync()     _unitOfWork.SaveChangesAsync()
_userRepository.SaveChangesAsync()       _unitOfWork.SaveChangesAsync()
_refreshTokenRepository.SaveChangesAsync()      (same DbContext underneath, now one name)

GET /health/live   -> process is up, zero dependency checks
GET /health/ready   -> process is up AND the database is reachable
```

---

# Progress Log

## Unit of Work

- **The problem this solves isn't "repositories can't save" - it's that they never needed their own `SaveChangesAsync` in the first place.** `ApplicationDbContext` is scoped per request and shared by every repository injected into a handler. Any one repository calling `SaveChangesAsync()` was already committing every pending change tracked by that shared context, not just "its own" entity's changes. Exposing `SaveChangesAsync` on `ITicketRepository`, `IUserRepository`, and `IRefreshTokenRepository` was misleading API design: it implied a repository owns its own commit, when the commit boundary is actually the `DbContext` (the request), not the repository.
- Fix: `IUnitOfWork` with a single `Task<int> SaveChangesAsync(CancellationToken ct = default)` method, implemented by wrapping `ApplicationDbContext.SaveChangesAsync`. Removed `SaveChangesAsync` from all three repository interfaces and implementations. Every handler that used to call `_someRepository.SaveChangesAsync()` now calls `_unitOfWork.SaveChangesAsync()` instead - same underlying call, but the name on the line now matches what's actually happening.
- All six Ticket handlers (Create/Update/Delete/Assign/ChangeStatus/OverridePriority) and all four Auth handlers (Register/Login/Refresh/Logout) updated.
- **A real bug surfaced while doing this, not something hunted for on purpose:** `RegisterCommandHandler` called `SaveChangesAsync` on two *different* repositories - `_userRepository` then `_refreshTokenRepository` - for one logical "register and issue a session" operation. A crash between those two calls would leave a persisted `User` with no session, an inconsistent state.
- **First fix attempt was wrong, and caught before it shipped:** collapsing both calls into a single `_unitOfWork.SaveChangesAsync()` at the very end doesn't work. `IssueAsync(user)` needs `user.Id` - an identity column - for both the JWT's claim and as `RefreshToken.UserId` (a plain `int` foreign key; `RefreshToken` has no navigation property back to `User`). EF Core only populates an identity column *after* `SaveChangesAsync` actually runs the `INSERT`. So the user has to be committed before `IssueAsync` can run at all.
- **What actually shipped:** `RegisterCommandHandler` still calls `_unitOfWork.SaveChangesAsync()` twice - once after `AddAsync(user)`, once after `IssueAsync`. This is an *honest*, not a *complete*, fix: the commit point is now consistent (one interface, not "whichever repository happened to be sitting there"), but true single-request atomicity for Register still isn't achievable without a schema change (e.g. giving `RefreshToken` a `User` navigation property EF Core could fix up in one `SaveChanges`, or generating the user id client-side with a GUID instead of an identity column). Documented in a code comment rather than silently left as a mystery two-liner.
- `LoginCommandHandler`, `RefreshCommandHandler`, `LogoutCommandHandler` all only needed the interface swap - none of them have Register's ordering problem, since the `User` they operate on already has a real `Id` from a prior `SELECT`.

## Health Checks

- Two endpoints, not one, because "is the app healthy" is actually two different questions with two different correct responses:
  - `GET /health/live` - liveness. Zero dependency checks (`Predicate = _ => false`). Answers "is the process itself alive and able to respond to HTTP at all?" If this fails, the process is wedged and the right response is to restart it.
  - `GET /health/ready` - readiness. Runs the `database` check (`AddDbContextCheck<ApplicationDbContext>`, tagged `"ready"`). Answers "can this instance currently do its job?" If this fails, a load balancer should stop routing new traffic here - but the process should *not* be restarted, because restarting it doesn't fix a down database and creates a restart storm on top of an existing outage.
- Conflating these into one `/health` endpoint is a common real-world mistake: an orchestrator configured to restart on any health-check failure will kill and respawn every instance the moment the database has a blip, which is the opposite of what you want during a database outage.
- `AddDbContextCheck<ApplicationDbContext>` (from `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`) does a real `CanConnectAsync` against the configured connection string - it isn't a no-op ping.
- Default ASP.NET Core health check output is just the plain-text status word. `HealthCheckResponseWriter` (API layer, since this is a transport/formatting concern, not a business rule) returns JSON with per-check name, status, description, and duration - the shape an on-call dashboard or `curl` during an incident actually needs, instead of a bare "Unhealthy" with no indication of which dependency failed.
- Both endpoints are intentionally unauthenticated (no `[Authorize]`, no `RequireAuthorization()`) - a load balancer or Kubernetes kubelet has no JWT to send, and gating the health probe behind auth would make the probe itself always fail closed.

---

# Concepts Learned

- **The commit boundary is the `DbContext`/request, not the repository.** A repository's job is translating between domain objects and persistence; deciding *when* to flush those changes to the database is a separate responsibility, and giving every repository its own `SaveChangesAsync` blurred that line even though, mechanically, they were always sharing one context and one transaction.
- **A real limitation, once found, is a fact to document - not a bug to hide or a problem to force-fix.** Register's two-commit shape can't be collapsed to one without changing how `User.Id` is generated. Recognizing *why* (identity column vs. needing the id before a second write) and writing that down is more valuable than either ignoring it or claiming a fix that isn't actually achievable with the current schema.
- **Liveness and readiness are different questions with different correct remediations.** Liveness failure -> restart. Readiness failure -> stop routing traffic, don't restart. Merging them into one health check throws away the information an orchestrator needs to react correctly.
- **A health check should exercise the real dependency, not just "is the endpoint reachable."** `AddDbContextCheck` opens a real connection - a health check that only returns 200 unconditionally isn't verifying anything and gives false confidence during an actual outage.

---

# Interview Questions Unlocked

- Why did every repository have its own `SaveChangesAsync` before, and why is that actually redundant given how `DbContext` lifetime scoping works in ASP.NET Core?
- Walk through the Register handler's two-commit limitation: why can't it be a single `SaveChangesAsync` call, and what schema change would make it possible?
- What's the difference between a liveness probe and a readiness probe, and what's the concrete failure mode of checking the database in a liveness check?
- Why shouldn't a health check endpoint require authentication?
- `AddDbContextCheck` vs. a custom `IHealthCheck` that runs `SELECT 1` - what's the actual difference, and when would you write a custom one instead?

---

# Technical Debt (Intentional)

- `RegisterCommandHandler` still performs two separate `SaveChangesAsync` calls for one logical operation - a known, documented, and currently-accepted gap in atomicity. Revisit only if a schema change (GUID-based user ids, or a `User` navigation property on `RefreshToken`) is warranted for other reasons; not worth a migration on its own just to close this gap.
- No integration test project exists yet, so the health check endpoints are verified manually (`curl http://localhost:<port>/health/live` and `/health/ready`) rather than by an automated test. This is the same gap Sprint 8 flagged for `GetPagedAsync` - health checks are now the second concrete feature where the missing integration test project is the honest reason coverage stops at "compiles and was checked by hand."
- No separate `HealthChecksUI` or historical health-check dashboard wired up - the two endpoints return current-instant JSON only. Fine for now; revisit if operational visibility over time becomes a real need.

---

# Future Improvements

- Unit of Work and Health Checks are both done. Logging (Serilog + correlation IDs) and API Versioning remain from the Phase 3 list.
- Build the integration test project flagged in Sprint 8 and again here - `GetPagedAsync` and the two health check endpoints are now two independent, concrete reasons to finally do it, not just a theoretical gap.
- If ticket volume or infra ever demands it, add a dedicated `IHealthCheck` for anything beyond raw DB connectivity (e.g. a downstream service, disk space, a message queue) - the two-endpoint liveness/readiness split already in place will host it without restructuring.
