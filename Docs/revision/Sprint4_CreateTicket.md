# Sprint 4 - Create Ticket

## Milestone

First feature in the Ticket domain (Phase 2). Establishes the vertical slice pattern for Tickets, and introduces business-driven priority calculation (Impact x Urgency, ServiceNow/ITIL-inspired) instead of letting the creator pick priority directly.

```
HTTP Request
      |
      v
TicketController.Create
      |
      v
CreateTicketCommandHandler
      |
      v
Ticket.RecalculatePriority() -> TicketPriorityPolicy.Calculate
      |
      v
TicketRepository
      |
      v
EF Core -> SQL Server
```

---

# Progress Log

## Domain layer - done

- `TicketImpact` / `TicketUrgency` enums (Low/Medium/High).
- `TicketPriorityPolicy`: a standalone static class holding the Impact x Urgency -> Priority matrix, deliberately pulled out of `Ticket.cs` itself to avoid the entity file absorbing every future self-contained business rule (SLA calculations, escalation rules, etc. will follow the same "policy" pattern later).
- `Ticket.Priority` setter locked to `private` - priority can now only ever be set via `RecalculatePriority()`, never assigned directly, making an inconsistent Impact/Urgency/Priority state unrepresentable.
- Fixed a pre-existing naming bug: `Ticket.AssignedByUser` renamed to `AssignedToUser` to match what the FK (`AssignedToUserId`) actually represents.

## Application layer - done

- `ITicketRepository`, `CreateTicketCommand`, `CreateTicketCommandValidator`, `CreateTicketResult`, `CreateTicketCommandHandler`.
- `Status` is not present on `CreateTicketCommand` at all - not just rejected at runtime, structurally impossible for a client to influence. Always set to `Open` by the handler.
- No `TicketErrors.cs` yet - Create Ticket has no real business-failure branch (only input validation, handled upstream by FluentValidation), so an empty errors file would be ceremony without purpose.

## Persistence layer - done

- `TicketConfiguration` fixed to match the `AssignedToUser` rename; both `CreatedByUser` and `AssignedToUser` FKs to `User` already had correct explicit `HasOne/WithMany/HasForeignKey` configuration from Sprint 0 scaffolding (the dual-FK-to-same-entity ambiguity was already handled correctly before this sprint).
- `TicketRepository` - same `AddAsync`/`SaveChangesAsync` shape as `UserRepository`.
- Migration: `AddTicketImpactAndUrgency`.

## API layer - done

- `TicketController.Create` - `[Authorize]` (any authenticated user, no role restriction).
- `GetUserId()` extension on `ClaimsPrincipal` added to `Extensions.cs` - pulled out now since every future Ticket endpoint needs "who is the current user," meeting the project's own bar for justified abstraction (3+ real use cases).
- `TicketResponse` - a deliberate, separate output shape from `CreateTicketResult`, so the public JSON contract can evolve independently of what the Application layer internally returns.

Bug caught and fixed during this story: `CreateTicketRequest` (a separate API-layer input DTO) was initially introduced, following general Clean Architecture instinct, without checking it against this codebase's own established convention (`AuthController` binds directly to `RegisterCommand`/`LoginCommand`, no separate request wrapper). The mismatch silently broke FluentValidation's auto-validation, since it only validates the exact bound parameter type - `CreateTicketRequest` had no validator, and empty titles/descriptions were accepted. Fixed by binding directly to `CreateTicketCommand` and overwriting `CreatedByUserId` after binding, matching the Auth pattern. `CreateTicketRequest.cs` deleted.

---

# Concepts Learned

- Rich domain model vs anemic entity: giving `Ticket` its own `RecalculatePriority()` method, rather than computing priority externally and assigning it, keeps the business rule attached to the data it governs.
- When *not* to reach for an interface: `TicketPriorityPolicy` is a pure, deterministic function with no I/O - it doesn't need `ITicketPriorityCalculator` + DI, because there's nothing to swap out and nothing that needs mocking.
- Making illegal states unrepresentable: locking `Priority`'s setter to `private`, and leaving `Status` off `CreateTicketCommand` entirely, are both the same idea applied twice - prevent a wrong state at compile time instead of rejecting it at runtime.
- FluentValidation's ASP.NET Core auto-validation only fires for the exact type bound as an action parameter - a Request/Command split silently disables validation unless a validator exists for *both* types, or the split is removed.
- A conscious, undecided architectural tension is fine to leave as-is, as long as the trigger for revisiting it is written down - not every real tradeoff needs resolving immediately (see Technical Debt below).

---

# Interview Questions Unlocked

- Why give `Ticket` a `RecalculatePriority()` method instead of computing priority in the handler?
- When would you *not* wrap a piece of logic in an interface, even in a codebase that otherwise uses DI everywhere?
- What's the difference between rejecting invalid input at runtime vs. making it structurally impossible to submit in the first place?
- Why did binding to a separate Request DTO break validation, and why didn't the same problem affect `RegisterCommand`/`LoginCommand`?
- Why keep `CreateTicketResult` and `TicketResponse` as two separate types when they currently look identical?

---

# Technical Debt (Intentional)

- **`CreateTicketCommand` doubles as both the HTTP wire contract and the Application-layer input type.** This is a real, named tension with Single Responsibility - the class now has two reasons to change (a business-driven field change, or an HTTP/JSON-contract-driven change) that could diverge. Deliberately left as-is for now, matching the existing `RegisterCommand`/`LoginCommand` convention, since there is no *current* concrete need for the two to differ. Revisit the moment any of these becomes true: the API needs a field the Application doesn't (or vice versa), two API versions need to share one handler, or the JSON contract needs framework-specific attributes that don't belong on an Application-layer class.
- **No test coverage yet for `CreateTicketCommandHandler`** (in progress - see below).

---

# Future Improvements

- `Domain/Policies/` folder is now an established pattern - expect more of these as the Ticket domain grows (SLA deadline calculation, escalation rules).
- Revisit the Request/Command coupling per the trigger conditions above.
- Once "Get Ticket" exists, its own result/response types should be created fresh, not by reusing `CreateTicketResult`/`TicketResponse`, per Vertical Slice convention.
