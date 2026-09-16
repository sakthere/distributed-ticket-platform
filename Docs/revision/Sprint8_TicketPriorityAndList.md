# Sprint 8 - Ticket Priority Override and List/Search Tickets

## Milestone

Phase 2 (Ticket Management) is complete. This sprint closes the last two open items: letting Admin/Agent re-prioritize a ticket after creation, and giving the API a real way to browse tickets instead of only fetching one at a time by id.

```
PATCH /api/tickets/{id}/priority          GET /api/tickets?page=&status=&sortBy=...
        |                                          |
        v                                          v
OverrideTicketPriorityCommandHandler      GetTicketsQueryHandler
        |                                          |
        v                                          v
Ticket.OverridePriority(impact, urgency)  ITicketRepository.GetPagedAsync(filter, ct)
        |                                          |
        v                                          v
SaveChangesAsync()                        EF Core: Where/OrderBy/Skip/Take/CountAsync
```

---

# Progress Log

## Ticket Priority Override

- `Ticket.OverridePriority(impact, urgency)` sets `Impact`/`Urgency` and calls the existing `RecalculatePriority()` - reused, not re-derived, from Sprint 4's `TicketPriorityPolicy`. Throws on a terminal status, same guard shape as `AssignTo`.
- `OverrideTicketPriorityCommandHandler` reuses the exact resource-based authorization shape introduced for Ticket Status in Sprint 7 (Admin always; the assigned Agent; or any Agent if the ticket is still unassigned, since re-triaging Impact/Urgency before assignment is the same real situation that justified the rule the first time). This is the second time that three-way rule has been needed - per the project's own Abstraction Rules (three concrete use cases before naming an abstraction), it's now worth asking whether a shared helper or a small policy object is warranted the next time a third case shows up, rather than copying the `isAdmin`/`isAssignedAgent`/`isUnassigned` block a third time.
- `TicketErrors.TicketPriorityNotEditable` (409).
- `PATCH /api/tickets/{id}/priority`, role-gated like Assign/ChangeStatus.

## List/Search Tickets

- **The second Query in this codebase** (after Get Ticket by id). `GetTicketsQueryHandler` validates pagination bounds (`Page >= 1`, `1 <= PageSize <= 100`) before anything else - an unbounded `PageSize` is a genuine performance/DoS concern for a list endpoint, not a nitpick.
- `ITicketRepository.GetPagedAsync(TicketListFilter, ct)` - the repository interface takes a closed DTO of primitives and enums, never an `IQueryable<Ticket>` and never a raw SQL fragment. This matters structurally: if the interface exposed `IQueryable`, the Application layer would be composing EF Core query expressions without depending on EF Core directly by name, which is exactly the kind of leaky abstraction Clean Architecture's dependency rule exists to prevent - the *type* wouldn't announce the coupling, but the coupling would still be there.
- `TicketListSortBy` is a closed enum (`CreatedAt`/`Priority`/`Status`), not a free-text sort-field string. The only way to make a new column sortable is to add a case to the enum *and* to the repository's switch - by construction, no client-supplied string can ever reach an `ORDER BY` clause.
- `PagedResult<T>` (`Application/Common`) - the first genuinely reusable generic type in this Application layer. Every future paged list (users, comments, anything) returns this same `Items`/`TotalCount`/`Page`/`PageSize`/`TotalPages` shape instead of each feature inventing its own envelope.
- Employees get `CreatedByUserId` forced to their own id in the handler, overriding whatever the client sent (or omitted) - same "ignore who's asking to be, use who's actually asking" shape as Get Ticket's ownership check, just applied to a filter instead of a single record.
- `TicketListItemResult`/`TicketSummaryResponse` are deliberately lighter than the full ticket shape (no `Description`) - a list view doesn't need it, and repeating it per row across a page of results is wasted payload for data the UI wouldn't show in a list anyway.
- `GET /api/tickets`, bound via `[FromQuery]` directly to `GetTicketsQuery` - consistent with this codebase's standing convention of binding straight to the Command/Query type and overwriting the security-sensitive fields (`CurrentUserId`/`CurrentUserRole`) after binding, rather than introducing a separate request DTO.

---

# Concepts Learned

- **Reusing an authorization shape is a signal, not just convenience.** The same `isAdmin || isAssignedAgent || isUnassigned` check now appears twice (Status, Priority). Recognizing the repeat - and explicitly deciding *not* to abstract it yet, waiting for a third case - is itself the skill; abstracting after one repeat is premature, abstracting after ignoring three real repeats is technical debt.
- **A closed filter DTO versus a leaky `IQueryable`.** Passing `IQueryable<Ticket>` out of a repository interface *looks* clean (Application never writes `using Microsoft.EntityFrameworkCore`) but isn't: the Application layer would still be shaping a query plan meant for a specific provider. A closed set of primitives is the actual boundary.
- **Pagination validation is a real security/performance control, not boilerplate.** Rejecting `PageSize > 100` up front, before touching the database, is the same "fail fast on cheap checks before expensive ones" principle from Sprint 6's Assign handler, applied to a resource-exhaustion concern instead of a business rule.
- **Total count and page slice are two separate queries against the same filtered set** (`CountAsync` then `Skip/Take`), and that's a real, known cost for large tables (some real-world designs use an approximate or cached count instead of a live one at scale) - not something this feature needed to solve yet, but worth knowing it's a tradeoff, not a given.

---

# Interview Questions Unlocked

- Why is exposing `IQueryable<T>` from a repository interface controversial in a Clean Architecture codebase, even though the Application layer never references EF Core by name if it does?
- Why validate `PageSize` server-side even though the client controls the request - what's the actual attack/failure this prevents?
- Walk through why `TicketListSortBy` is an enum and not a string - what's the concrete risk of a string, given this is LINQ-to-EF-Core and not raw SQL (i.e., SQL injection isn't literally possible here - so what *is* the risk)?
- You've now written the same three-way authorization check twice. When do you extract it, and into what - a shared private method, a static policy class, a base handler? What are the tradeoffs of each?
- Why does a paged list response need a separate `TotalCount` field at all, instead of just returning `Items.Count`?

---

# Technical Debt (Intentional)

- The `isAdmin`/`isAssignedAgent`/`isUnassigned` authorization check is now duplicated across `ChangeTicketStatusCommandHandler` and `OverrideTicketPriorityCommandHandler`. Explicitly not extracted yet (two occurrences, and the project's own Abstraction Rules ask for three before naming something) - the trigger to revisit is the *next* handler that needs the same shape.
- `TicketRepository.GetPagedAsync`'s actual filter/sort/paging LINQ is untested by anything in this codebase - every handler test mocks `ITicketRepository`, so the EF Core query logic itself (Skip/Take math, the sort-enum switch, the `Where` chain) is unit-test-blind. This is the same gap that's existed since Sprint 2's "no integration tests" decision, but it's the first feature where getting that logic wrong would be silent and only show up against a real database with real data volume - the strongest concrete case yet for finally building integration tests.
- No cursor-based (keyset) pagination - offset-based `Skip/Take` was chosen for simplicity, and is fine at this table's current size, but is a known scaling concern (large offsets get progressively slower). Revisit if/when ticket volume or performance data says so.

---

# Future Improvements

- Phase 2 (Ticket Management) is done. Phase 3 (Engineering Improvements) is next per the roadmap: Unit of Work, Logging, Health Checks, API Versioning (Global Exception Middleware and the Result pattern are already in place from earlier work).
- Actually build integration tests, using `GetPagedAsync` as the first real target - it's the clearest example yet of behavior that unit tests structurally cannot verify.
- Revisit the duplicated three-way authorization check the moment a third handler needs it.
