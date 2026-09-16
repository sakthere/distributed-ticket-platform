# Sprint 5 - Update Ticket

*Written retroactively during Sprint 6, filling a gap - this recap wasn't created when the feature actually shipped and merged (PR #2, commit `3945831`). Reconstructed from the design discussion and the merged code, not from a live walkthrough, so treat the "why" here as accurate but slightly less granular than a recap written in the moment.*

## Milestone

Second feature in the Ticket domain. Establishes the ownership + status-guard pattern that Delete Ticket (Sprint 6) later extends into a hybrid owner-or-admin rule.

```
PATCH /api/tickets/{id}
        |
        v
UpdateTicketCommandHandler
        |
        v
Ticket.UpdateDetails(title, description)   <- throws if Status != Open
        |
        v
ITicketRepository.SaveChangesAsync()
```

---

# Progress Log

## Business/Functional shape

Only `Title` and `Description` are editable, and only while the ticket is still `Open` - modeled after ServiceNow-style ITSM systems, which similarly lock down editing once a ticket has moved into active work. `Impact`/`Urgency` (and therefore the derived `Priority`) are deliberately *not* editable by the ticket creator through this endpoint - resetting them is scoped as something only the Admin/Agent actively working the ticket should be able to do, not the original reporter. That's a future story, not built here.

## Domain layer

- `Ticket.UpdateDetails(string title, string description)`: throws `InvalidOperationException` if `Status != Open`, otherwise updates both fields. Same defense-in-depth shape used again in Sprint 6 for `AssignTo` - the entity-level guard is a backstop for a bug in the caller, not the primary way this rule gets enforced for a normal user.

## Application layer

- `UpdateTicketCommand`, `UpdateTicketResult`, `UpdateTicketCommandHandler`.
- Check order in the handler: not found -> not the owner (403) -> not editable in current status (409). Ownership is checked before status deliberately - a non-owner gets a 403 regardless of the ticket's status, rather than leaking status information (a 409 vs 404 distinction) to someone who has no business seeing the ticket at all.
- `TicketErrors.NotTicketOwner` (403) and `TicketErrors.TicketNotEditable` (409) added - the second and third typed errors in the Tickets domain, after `NotFound`.

## API layer

- `TicketController.Update` - `PATCH /api/tickets/{id}`, `[Authorize]` (any authenticated user; ownership is enforced in the handler, not via a role attribute, since "own tickets only" isn't a role concept).
- PATCH chosen over PUT: this is a partial update (two fields out of the full ticket shape), and PUT's REST semantics imply a full resource replacement.

## Testing

- `UpdateTicketCommandHandlerTests` - four tests: owner updates an Open ticket (success), ticket not found, caller is not the owner, ticket is not Open. Written directly rather than by hand-typing, under a narrowly-scoped one-off exception to the project's normal "you type the code, I review it" rule - Saket explicitly chose that path for this one story only, and standard mode resumed immediately afterward for what would have been the next feature (before scope expanded again in Sprint 6).

Bugs caught and fixed while this file was open for an unrelated reason (adding the `Update` action):

- `TicketController.Create`'s failure branch was missing a `return` before `result.Error.ToActionResult()`, meaning a failed `Create` would fall through and throw a `NullReferenceException` instead of returning a clean error response.
- `TicketRepository.GetByIdAsync` was declared to return `Task` instead of `Task<Ticket?>`, and its body awaited the query but discarded the result instead of returning it - this should have been a compile error against `ITicketRepository`.

---

# Concepts Learned

- Ownership-before-status check ordering, and *why* the order matters for what information a 403 vs a 409 leaks to an unauthorized caller.
- PATCH vs PUT vs POST as a real design decision, not a convention followed on autopilot: PATCH signals partial mutation, matching the fact that only two of the ticket's fields are ever editable here.
- The same "guard rule lives as a `Result` check in the handler, and again as a thrown exception in the entity" pattern first introduced here, later reused for `AssignTo` in Sprint 6 - established as a repeatable convention rather than a one-off choice.
- 404 vs 403 vs 409 as a deliberate, system-wide mapping convention (401 = unauthenticated, 403 = authenticated but disallowed, 404 = not found or deliberately hidden, 409 = valid request but wrong resource state) - discussed with real-world examples (GitHub/S3 return 404 rather than 403 for private repos/objects specifically to avoid confirming they exist to someone with no access).

---

# Interview Questions Unlocked

- If `UpdateTicketCommandHandler`'s status check has a bug and lets a non-Open ticket through, what actually stops the update - and what HTTP status code does the caller see as a result? Walk through both paths (handler-catches-it vs. handler-misses-it-and-the-entity-throws).
- Why check ownership before status, and not the other way around?
- When would you choose to return 404 instead of 403 for an authorization failure, even though the resource genuinely exists?
- Why does this project throw exceptions for some business-rule violations and return `Result.Failure` for others, instead of picking one mechanism everywhere?

---

# Technical Debt (Intentional)

- No recap doc existed for this sprint until Sprint 6 - now fixed, but a reminder that "write the recap" should happen as part of finishing a story, not get silently skipped when moving fast between features.
- Same integration-test and resource-based-authorization debt as recorded in Sprint 2/3 - not repeated in full here, see Sprint 6's Technical Debt section for the current, consolidated state.

---

# Future Improvements

- `Impact`/`Urgency` reset by Admin/Agent is still an open, unbuilt story - worth scoping explicitly once Role Based Authorization Policies (roadmap) exist to gate it properly.
