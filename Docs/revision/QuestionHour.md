# Question Hour — Decision Reasoning Log

This is not a code reference. It's a memory drill. Each entry is a decision we made, with the reasoning behind it written out in full. Review this daily. Cover the "Reasoning" column with your hand (or just don't scroll) and try to answer the question first, in your own words, before reading it.

New decisions get added here as we build. Old ones don't get deleted — repetition over time is the point.

---

## How to use this

1. Pick 2-3 questions you haven't looked at in a few days.
2. Answer out loud or in writing, without looking at the reasoning.
3. Check yourself against the reasoning below.
4. If you got it wrong or vague, that's the one to revisit — not the ones you already know.

I'll randomly ask you about entries from here during check-ins. Getting one wrong isn't a problem — it's the point of asking.

---

## Sprint 1 — Registration & Foundations

**Q: Why Clean Architecture instead of one project with everything in it?**

Reasoning: A single project mixes business rules with framework code. Change EF Core, and you risk breaking business logic that had nothing to do with the database. Clean Architecture forces a dependency direction — Application never references ASP.NET Core or EF Core — so business rules can be tested and reasoned about without spinning up a web server or a real database. The cost is more files and more indirection. That cost is worth it once a codebase outlives a few months or has more than one contributor.

**Q: Why Vertical Slice organization (folders by feature, e.g. `Features/Authentication/Login`) instead of folders by technical layer (`Controllers/`, `Services/`, `Repositories/`)?**

Reasoning: Layer-first folders scatter one feature across five directories, so understanding "how Login works" means jumping all over the codebase. Feature-first folders keep everything for one use case together. It also limits blast radius — changing Login shouldn't require touching a shared "AuthService" that Register also depends on.

**Q: Why CQRS (Commands vs Queries) instead of one generic service method?**

Reasoning: Reads and writes have different concerns — writes need validation, business rules, and transactional safety; reads just need to shape data efficiently, sometimes bypassing the domain model entirely. Splitting them means each side can evolve independently. A read can query a flat DTO from the database directly; a write has to go through the full domain safety.

**Q: Why does the Repository Pattern exist, and why not just inject `DbContext` and call `.Users` directly in the handler?**

Reasoning: The Application layer isn't allowed to know EF Core exists (Dependency Inversion). If handlers called `DbContext` directly, Application would depend on Persistence and the whole point of Clean Architecture breaks. The repository interface lives in Application; the implementation lives in Persistence. It also makes swapping the data source (or writing a fake for a unit test) possible without touching business logic.

**Q: Why hash passwords with BCrypt instead of storing them, or using something like SHA-256?**

Reasoning: Storing plaintext means one DB leak exposes every user's real password. SHA-256 is a fast hash — attackers can brute-force billions of guesses a second on cheap hardware. BCrypt is deliberately slow (it has a configurable "work factor") and includes salting, so brute-forcing becomes computationally expensive even after a leak. Speed is a feature for legitimate logins (small delay per user) but a liability for attackers (huge delay across millions of guesses).

**Q: Why FluentValidation instead of DataAnnotations attributes on the model?**

Reasoning: DataAnnotations couple validation rules directly onto the model class, so complex or conditional rules get awkward fast. FluentValidation keeps validation as a separate, composable, testable class — and it's not tied to the model shape, so the same entity can have different validation rules in different contexts (e.g. Register vs. AdminCreateUser).

**Q: What's the difference between input validation and business validation, and why does it matter which layer owns each?**

Reasoning: Input validation checks shape — is the email formatted correctly, is the password long enough. This can happen before you touch the database. Business validation checks meaning — does this email already exist, is this user allowed to do this. That requires querying state. Input validation belongs at the edge (FluentValidation, before the handler runs); business validation belongs inside the handler, because only the handler has the context (and the repository) to check it.

---

## Sprint 2 — Refresh Token Authentication

**Q: Why can't a JWT be revoked once it's issued?**

Reasoning: A JWT is stateless by design — the server verifies it using a signature, not a database lookup. That's the entire performance benefit: no DB hit per request. But it means the server has no "off switch." If someone steals a valid JWT, it stays valid until it naturally expires, no matter what the server does.

**Q: If JWTs can't be revoked, why not just make them short-lived and call it done?**

Reasoning: Because that alone creates a UX problem — a user gets logged out mid-task every few minutes. Making it long-lived instead fixes UX but reopens the original problem: an attacker with a stolen long-lived token has a long window to do damage. Refresh tokens split the difference: a short-lived, stateless access token for actual requests, and a long-lived, stateful (DB-tracked, revocable) refresh token whose only job is minting new access tokens. You get both short attacker windows and a way to kill a session outright.

**Q: What is refresh token rotation, and what does it defend against?**

Reasoning: Every time a refresh token is used, the old one is immediately marked dead and a new one is issued in its place — even though the old one hadn't expired yet. This means a refresh token is single-use. If an attacker steals a refresh token and the legitimate user refreshes first, the attacker's copy is now dead on arrival. It defends against silent, long-term token theft, where an attacker sits on a stolen token and uses it occasionally without the real user noticing.

**Q: A rotated-out (dead) refresh token gets replayed. Why revoke the entire session family, not just that one token?**

Reasoning: A dead token being replayed is the strongest possible signal of theft — the *legitimate* rotation chain would never present an already-used token, because each valid client always has the latest one. The only way an old token appears is if someone made an unauthorized copy before rotation happened. At that point, you can't trust anything downstream in that chain either, because you don't know how many tokens the attacker copied. Killing only the replayed token leaves the current, "legitimate-looking" token active — which might actually be in the attacker's hands, not the user's. Revoking the whole family (via `SessionId`) is the only response that's actually safe.

**Q: Why hash the refresh token before storing it (SHA-256), when we already know BCrypt for passwords — why not reuse BCrypt here?**

Reasoning: Passwords are low-entropy — humans pick short, memorable, guessable strings, so hashing needs to be deliberately slow to resist brute-forcing. A refresh token is a 64-byte cryptographically random value — there's nothing to "guess," the entropy is already enormous. BCrypt's slowness would just be wasted cost (and a real one, since every refresh call would hash on the hot path). SHA-256 is fast and sufficient here because the attack it defends against (a DB leak exposing raw tokens) doesn't rely on brute-force resistance — the token was never guessable to begin with.

**Q: Why generate the raw token with `RandomNumberGenerator` instead of `Guid.NewGuid()`?**

Reasoning: `Guid.NewGuid()` is not guaranteed to be cryptographically secure — its randomness comes from a general-purpose algorithm, not one designed to resist prediction. `RandomNumberGenerator` pulls from the OS's cryptographic random source, which is what you want for anything security-sensitive like a session token, where predictability would be catastrophic.

**Q: Why HttpOnly + Secure + SameSite=Strict cookies for the refresh token instead of returning it in the response body and letting the client store it (e.g. localStorage)?**

Reasoning: `localStorage` is readable by any JavaScript running on the page — including injected malicious scripts (XSS). A stolen refresh token from localStorage is a long-lived, high-value target. `HttpOnly` cookies are invisible to JavaScript entirely, closing that attack surface. `Secure` ensures the cookie only ever travels over HTTPS. `SameSite=Strict` stops the browser from attaching the cookie on requests originating from other sites, which closes most CSRF vectors. The tradeoff: cookies open a (smaller, mitigated) CSRF surface in exchange for closing a much larger XSS surface — and CSRF is easier to defend against directly (SameSite, anti-forgery tokens) than XSS is to fully prevent in a real app with any third-party JS.

**Q: Why does `AuthSessionIssuer.IssueAsync` NOT call `SaveChangesAsync` itself?**

Reasoning: Token rotation needs to revoke the old token AND create the new one as a single atomic operation. If `IssueAsync` saved immediately, the caller (`RefreshCommandHandler`) would have no way to bundle "mark old token revoked" and "insert new token" into one transaction — you'd risk a crash between the two calls leaving the system in a half-rotated state. By staging the change and letting the caller decide when to commit, the caller controls the transaction boundary.

**Q: In `RefreshCommandHandler`, why check `IsRevoked` before checking `IsExpired`?**

Reasoning: Both are technically "invalid token" states, but they mean very different things. Expired just means time passed — normal, not suspicious. Revoked-and-replayed means someone used a token that should no longer exist — which is a signal of theft, not just staleness. If you checked expiry first, a revoked-but-not-yet-expired token replay would get a generic "expired" response — masking a security event as a mundane one. Checking revocation first ensures theft gets flagged as theft.

---

## Sprint 3 — Role-Based Authorization

**Q: What's the difference between authentication (401) and authorization (403), and why is conflating them a common interview mistake?**

Reasoning: Authentication answers "who are you" — a 401 means the server doesn't know, because no valid credential (or token) was presented at all. Authorization answers "are you allowed to do this" — a 403 means the server knows exactly who you are, and the answer is still no. Candidates conflate them because both look like "access denied" from the outside, but they happen at different points in the pipeline and communicate different things to the client: 401 says "log in," 403 says "you're logged in, but this isn't for you."

**Q: Why use role-membership authorization (`[Authorize(Roles=...)]`) here instead of building a resource-based policy right away?**

Reasoning: Role-membership authorization is attribute-based and needs no data — it just checks a claim on the token. Resource-based authorization (e.g. "only the assigned Agent can update this ticket") needs a custom handler that inspects the actual resource, which means there has to be real resource data to check against. Building the resource-based version before the Ticket domain existed would mean guessing at a shape with no evidence. Proving the cheap mechanism first, in isolation, was the deliberate scope.

**Q: If you revoke a user's Admin role right now, are they still Admin? Why?**

Reasoning: Yes, until their current access token expires. This is the same stateless-JWT tradeoff as refresh tokens, showing up in a new place — the role is a claim baked into the token at issue time, not re-checked against the database on every request. Revoking the role in the DB doesn't retroactively change a token that's already out in the world.

**Q: Why use `nameof(UserRole.Admin)` instead of the string `"Admin"` directly in the `[Authorize(Roles=...)]` attribute?**

Reasoning: A hardcoded string has no connection to the enum — rename or remove the enum member, and the string silently goes stale, either locking out or letting in the wrong people at runtime with no warning. `nameof()` ties the attribute to the enum at compile time, so a rename breaks the build loudly instead of failing silently in production.

---

## Sprint 4 — Create Ticket

**Q: Why give `Ticket` its own `RecalculatePriority()` method instead of computing priority in the handler and assigning it?**

Reasoning: This is the rich-domain-model vs anemic-entity distinction. If the handler computes priority and assigns it externally, the entity becomes a passive data bag and the business rule lives somewhere disconnected from the data it governs. Giving `Ticket` the method keeps the rule attached to the entity, and — combined with a `private` setter on `Priority` — makes it structurally impossible to end up with a `Ticket` whose Priority doesn't match its Impact/Urgency.

**Q: `TicketPriorityPolicy` is a plain static class with no interface, in a codebase that otherwise uses DI everywhere. Why no `ITicketPriorityCalculator`?**

Reasoning: Interfaces exist to allow swapping an implementation or mocking it in a test. `TicketPriorityPolicy` is a pure, deterministic function — same inputs always produce the same output, no I/O, nothing to fake. Wrapping it in an interface would add indirection with nothing to show for it. The project's own abstraction rule (only introduce one with a real, current need) applies here.

**Q: Why is `Status` left off `CreateTicketCommand` entirely, instead of accepting it and validating that it's always `Open`?**

Reasoning: Same idea as the `Priority` setter — making illegal states unrepresentable instead of rejecting them at runtime. If `Status` were on the command, a client could technically send `Status: Closed` on creation, and the code would need a runtime check to reject it. Leaving it off the command means a client can't even construct that illegal request — the handler just always sets it to `Open`. Compile-time prevention beats runtime rejection.

**Q: Why did binding the controller to a separate `CreateTicketRequest` DTO silently break validation, when `AuthController` binding directly to `RegisterCommand`/`LoginCommand` works fine?**

Reasoning: ASP.NET Core's FluentValidation auto-validation only fires for the exact type bound as the action parameter. `RegisterCommand` has a validator and is bound directly, so it validates. `CreateTicketRequest` was a new type with no validator of its own — the validator existed for `CreateTicketCommand`, a different type, so nothing ran. It wasn't a framework bug, it was introducing a Clean-Architecture-instinct pattern (separate wire DTO) without checking it against this codebase's established convention first. Fixed by binding directly to the command, matching the existing pattern.

**Q: `CreateTicketResult` and `TicketResponse` currently have identical shapes. Why keep them as two separate types instead of one?**

Reasoning: They serve different contracts even if they look the same today. `CreateTicketResult` is what the Application layer returns internally; `TicketResponse` is the public JSON contract the API exposes. Collapsing them into one type means any future change to either — an internal field the API shouldn't expose, or a JSON-shape change the Application layer doesn't care about — forces a change in a place it doesn't belong. Keeping them separate costs a small amount of duplication now in exchange for the two being free to diverge later without a breaking change on either side.

---

## Self-Revision Checklist (cumulative)

- [ ] Explain why Clean Architecture's dependency rule matters, with a concrete example of what breaks without it.
- [ ] Explain CQRS and why reads and writes benefit from separation.
- [ ] Explain the Repository Pattern and Dependency Inversion together — they're the same idea from two angles.
- [ ] Explain why BCrypt for passwords but SHA-256 for refresh tokens — same principle (hash before storing), different threat model.
- [ ] Explain refresh token rotation and reuse detection end-to-end, including why family-level revocation is necessary.
- [ ] Explain the XSS-vs-CSRF tradeoff behind HttpOnly cookies.
- [ ] Explain why `IssueAsync` doesn't save changes itself, and what atomicity problem that solves.
- [ ] Explain why revocation is checked before expiry in the refresh handler.
- [ ] Explain 401 vs 403 with a concrete example of each.
- [ ] Explain why revoking a role doesn't take effect immediately, tying it back to the JWT statelessness tradeoff.
- [ ] Explain rich domain model vs anemic entity using `Ticket.RecalculatePriority()`.
- [ ] Explain "illegal states unrepresentable" with two examples from Sprint 4 (`Priority` setter, `Status` omission).
- [ ] Explain why the Request/Command split broke validation, and why the fix matched an existing convention instead of inventing a new one.

---

*Last updated: Sprint 4 (Create Ticket). Add new entries here as each future feature is completed — same format: Question, then Reasoning.*
