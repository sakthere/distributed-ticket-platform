# Sprint 3 - Role-Based Authorization

## Milestone

Prove role-based access control end-to-end using the JWT's existing role claim, before any real Ticket endpoint depends on it.

---

# Progress Log

## API layer - done

- `TestController.AdminOnly()`: `[Authorize(Roles = nameof(UserRole.Admin))]`, a disposable proof-of-concept endpoint.
- No Domain, Application, or Infrastructure changes needed - the JWT already carried a role claim since Sprint 1/2 (`JwtTokenGenerator` line 26), and `AddAuthorization()` was already registered in `Program.cs`. This story was scoped deliberately small: prove the mechanism cheaply and in isolation before real endpoints are built on top of it.

Verified manually: unauthenticated request -> 401, authenticated Employee -> 403, manually-promoted Admin -> 200.

---

# Concepts Learned

- The difference between authentication (401, "I don't know who you are") and authorization (403, "I know who you are, and no") - and why conflating them is a common interview mistake.
- Role-membership authorization (`[Authorize(Roles=...)]`, attribute-based, no data involved) vs resource-based authorization (a policy with a custom handler that inspects the actual resource, e.g. "only the assigned Agent"). Deliberately did not build the second kind yet - no real Ticket data exists to check against, so building it now would be guessing at a shape without evidence.
- Same stateless-JWT tradeoff as refresh tokens, in a new place: revoking someone's Admin role doesn't take effect until their current access token expires, because the role is a claim baked into the token, not re-checked against the database per request.
- `nameof(UserRole.Admin)` over a hardcoded magic string - ties the attribute to the enum at compile time, so a rename breaks the build loudly instead of a typo silently locking out (or letting in) the wrong people at runtime.

---

# Interview Questions Unlocked

- What's the difference between 401 and 403, and when does each get returned?
- When would you reach for policy-based authorization instead of `[Authorize(Roles=...)]`?
- If you revoke a user's Admin role right now, are they still Admin? Why?
- How would you seed the very first Admin user in a real system, given signup can only ever create Employees?
- Why use `nameof(UserRole.Admin)` instead of the string `"Admin"` directly in the attribute?

---

# Technical Debt (Intentional)

- **No seeding mechanism for the first Admin user.** The only way to create one right now is a manual `UPDATE Users SET Role = 3 WHERE Email = '...'` in the database directly - `Register` deliberately only ever creates `Employee` accounts (letting self-registration grant Admin would defeat the entire point of role-based access control). A real system needs an out-of-band bootstrap: a startup seed step, a migration, or a locked-down internal endpoint gated by a deploy-time secret. Revisit once the Ticket domain exists and an actual Admin workflow (e.g. assigning Agents) gives this real urgency.
- **403 responses bypass the `Result`/`ProblemDetails` pipeline.** `[Authorize]` runs before the controller action, so a role-check failure returns ASP.NET Core's bare default 403 body instead of the shaped `ProblemDetails` the rest of the API returns. Cosmetic inconsistency, not a security gap - low priority.
- **`TestController.AdminOnly()` is scaffolding.** Expect it to be deleted or replaced once a real Ticket endpoint exists to enforce a role on directly.

---

# Future Improvements

- Authorization Policies (resource-based: "only the assigned Agent can update this ticket") once the Ticket domain has real data to check against.
- A proper Admin-seeding mechanism (see tech debt above).
- Shape `[Authorize]`-triggered 401/403 responses through the same `ProblemDetails` format as the rest of the API, for consistency.
