# Sprint 7 - Ticket Status Transitions

## Milestone

The ticket lifecycle becomes an explicit state machine instead of an implicit side effect of other features. Before this sprint, `Status` only ever changed as a byproduct of Create (`Open`) and Assign (`Assigned`) - there was no way for an Agent to actually move a ticket through `InProgress -> Resolved -> Closed`, or reject it.

```
PATCH /api/tickets/{id}/status
        |
        v
ChangeTicketStatusCommandHandler
        |
        v
Ticket.ChangeStatus(newStatus)   <- throws if TicketStatusPolicy says the transition is illegal
        |
        v
ITicketRepository.SaveChangesAsync()
```

---

# Progress Log

## Domain layer

- `TicketStatusPolicy` (`Domain/Policies`) - a static transition table, same shape as `TicketPriorityPolicy`: pure, deterministic, no I/O, no interface needed.
  - `Open -> {Assigned, Rejected}`
  - `Assigned -> {InProgress, Rejected}`
  - `InProgress -> {Resolved, Rejected}`
  - `Resolved -> {Closed}`
  - `Closed -> {}`, `Rejected -> {}` (both terminal)
  - Deliberately no "reopen" transition (nothing out of `Resolved`/`Closed`/`Rejected` back into an earlier state) - no story has asked for it yet, and adding one later is a one-line change to the table, not a redesign.
- `Ticket.ChangeStatus(TicketStatus newStatus)` - throws if the transition isn't in the table. Same defense-in-depth shape as `UpdateDetails`/`AssignTo`: the handler is expected to have already rejected an illegal transition via `Result`, so reaching this guard means the caller has a bug.

## Bug found and fixed while building this

`Ticket.AssignTo` unconditionally set `Status = Assigned` on every call - including when reassigning a ticket that was already `Assigned` or `InProgress` to a *different* agent. That silently reset a ticket's progress back to `Assigned` every time ownership changed hands, which would have actively fought this sprint's new state machine (an agent hands off an `InProgress` ticket, and it snaps back to `Assigned` for no reason). Fixed: only the *first* assignment - when the ticket is still `Open` - transitions status. Reassigning an already-`Assigned`/`InProgress` ticket now just changes `AssignedToUserId` and leaves `Status` alone. Covered by an updated test and a new one in `AssignTicketCommandHandlerTests`.

## Application layer

- `ChangeTicketStatusCommand`, `ChangeTicketStatusResult`, `ChangeTicketStatusCommandHandler`.
- Authorization here is genuinely hybrid, and different from anything built so far:
  - `[Authorize(Roles = Agent,Admin)]` on the endpoint - same role gate as Assign.
  - The handler then requires the caller be **either** an Admin, **or** the specific Agent the ticket is assigned to, **or** (the interesting case) *any* Agent, if the ticket is still unassigned. That last branch matters: without it, an unassigned `Open` ticket could only ever be rejected by an Admin, because no Agent could satisfy "is the assigned agent" on a ticket nobody's claimed. That's not how triage works in practice - any Agent should be able to look at an unclaimed ticket and reject it as invalid/duplicate before it's ever assigned.
  - `TicketErrors.NotAssignedAgent` (403) is the failure case; deliberately a new, differently-worded error from `NotTicketOwner`, since "you don't own this ticket" (creator ownership) and "you're not the agent working this ticket" are different facts about the caller.
- Check order: not found -> resource authorization -> transition validity (`TicketErrors.InvalidStatusTransition`, 409) -> mutate + save. Same "authorization before state" ordering used everywhere else in this codebase, so an unauthorized caller never learns anything about the ticket's current status.
- `TicketErrors` gained `InvalidStatusTransition` (TICKET007) and `NotAssignedAgent` (TICKET008).

## API layer

- `PATCH /api/tickets/{id}/status`, role-gated the same way as `Assign`.

## Testing

`ChangeTicketStatusCommandHandlerTests`: assigned Agent making a valid transition, Admin acting on a ticket assigned to a different Agent, any Agent rejecting an unassigned Open ticket, ticket not found, a different (non-assigned) Agent blocked with `NotAssignedAgent`, and a `[Theory]` covering five illegal transitions - skipping a state (`Open -> Resolved`, `Open -> InProgress`), going backwards from a later state to an earlier terminal-adjacent one (`Assigned -> Closed`), and out of both terminal states (`Closed -> Open`, `Rejected -> Open`).

---

# Concepts Learned

- **A state machine as data, not a chain of `if`s.** `TicketStatusPolicy`'s dictionary-of-arrays is the whole rulebook in one place - adding, removing, or re-scoping a transition is a one-line change, and there's no risk of an `if/else` chain silently missing a case the way scattered checks would.
- **Resource-based authorization can have more than two outcomes.** Update/Delete's ownership checks were binary (owner or not). This one has three: Admin (always), the specific assigned Agent (usually), and *any* Agent (only in the unassigned edge case) - modeling "who's allowed" sometimes needs more than a single boolean.
- **A bug found by writing the next feature, not by testing the feature that had it.** `AssignTo`'s reset-on-reassignment bug was latent since Sprint 6 and passed every existing test, because no test happened to reassign an already-in-progress ticket and then check its status didn't move. It only became obviously wrong once a real state machine existed to contradict it.

---

# Interview Questions Unlocked

- How would you model a multi-state workflow (ticket lifecycle, order fulfillment, approval chain) in code? Why a lookup table over a chain of conditionals, and when would the reverse be true?
- Walk through why "any Agent can act on an unassigned ticket, but only the assigned Agent once it's claimed" is a legitimate authorization rule, not a bug - what real-world workflow does it encode?
- This bug in `AssignTo` passed all existing tests. What does that tell you about test coverage versus test *scenario* coverage - and how would you have caught it earlier?
- Why introduce a new error (`NotAssignedAgent`) instead of reusing `NotTicketOwner`, when both are "you can't touch this ticket" 403s to the caller?

---

# Technical Debt (Intentional)

- No "reopen" transition (`Resolved`/`Closed`/`Rejected` back to an earlier state) - not built because nothing has asked for it. Revisit the moment a real workflow needs it; the table's shape already supports adding it cheaply.
- `TicketResponse` is now shared across five endpoints (Create, Update, Assign, Get, and now ChangeStatus) - the same open question flagged in Sprint 6 remains open, now with one more data point in favor of eventually deciding it.
- Resource-based authorization is now three-shaped (Update/Delete's owner-or-admin, and this sprint's admin-or-assigned-agent-or-unassigned). Still hand-rolled `Result` checks per handler, not `IAuthorizationHandler` policies - the "2-3 rules" revisit trigger from Sprint 3/6 is now clearly past due.

---

# Future Improvements

- Actually revisit policy-based resource authorization (see Technical Debt - this has been flagged three sprints running).
- Ticket Priority override (Admin/Agent resetting Impact/Urgency post-creation) is the next roadmap item and the last piece of Phase 2 left unbuilt before List/Search Tickets.
- Consider whether `ChangeStatus`'s three-way authorization shape (admin / assigned-agent / unassigned-anyone) is common enough across future features to name and reuse, or whether it's a one-off specific to this workflow - don't abstract it until a second real case shows up (per the project's own Abstraction Rules).
