# Sprint 2 - Refresh Token Authentication Vertical Slice

## Milestone

Extend Authentication with refresh tokens: rotation, replay detection, revocation, and multi-device sessions - without letting the JWT itself become long-lived.

```
HTTP Request
      │
      ▼
AuthController (register / login / refresh / logout)
      │
      ▼
RegisterCommandHandler / LoginCommandHandler / RefreshCommandHandler / LogoutCommandHandler
      │
      ▼
AuthSessionIssuer  (Application) ── IJwtTokenGenerator, IRefreshTokenGenerator, IRefreshTokenHasher (Infrastructure)
      │
      ▼
RefreshTokenRepository (Persistence)
      │
      ▼
EF Core → SQL Server
```

---

# Progress Log

## Domain layer - done

- Redesigned `RefreshToken` entity: `TokenHash` (not raw token), `ExpiresAt` (not a stored `IsExpired` bool), `SessionId` (rotation-chain grouping), `IsRevoked`/`RevokedAt`.
- `IsExpired` and `IsActive` are computed properties, not stored columns.

## Application layer - done

- New interfaces: `IRefreshTokenRepository`, `IRefreshTokenGenerator`, `IRefreshTokenHasher`.
- `IJwtTokenGenerator.GenerateToken` now returns `(Token, ExpiresAt)` instead of just a string.
- `IUserRepository` gained `GetByIdAsync` (needed to remint claims during refresh).
- New shared service `IAuthSessionIssuer` / `AuthSessionIssuer` - used by Register, Login, and (soon) Refresh. Stages a new refresh token but does **not** call `SaveChangesAsync` itself; the caller controls when the save happens, so rotation can be atomic later.
- `AuthErrors` gained `InvalidRefreshToken`, `RefreshTokenExpired`, `RefreshTokenReused`.
- `RegisterCommandHandler` and `LoginCommandHandler` now issue a full session (access + refresh token) via `AuthSessionIssuer`. Register now auto-logs the user in after signup (previously it only returned the new `UserId`).

## Infrastructure layer - done

- `JwtTokenGenerator` now returns `(Token, ExpiresAt)`; fixed the `DateTime.Now` → `DateTime.UtcNow` bug from Sprint 1 while touching this line anyway.
- `RefreshTokenGenerator`: cryptographically random token via `RandomNumberGenerator` (not `Guid.NewGuid()`), URL-safe base64, expiry driven by `RefreshTokenSettings.ExpiryDays`.
- `RefreshTokenHasher`: SHA-256, unsalted (deliberate - token is already high-entropy, unlike a password).

## Persistence layer - done

- `RefreshTokenConfiguration`: unique index on `TokenHash` (lookup path), index on `SessionId` (revoke-family path), `Cascade` delete on the `User` FK (unlike `Ticket`'s `Restrict` - a session has no meaning without its user).
- `RefreshTokenRepository`: `RevokeSessionFamilyAsync` uses `ExecuteUpdateAsync` to write immediately, not staged - a theft response shouldn't wait on an unrelated `SaveChangesAsync`.
- `UserRepository.GetByIdAsync` added, needed to remint claims during rotation.

## API layer - done

- `AuthController`: added `POST /refresh` and `POST /logout`; `Register`/`Login` now set the refresh-token cookie.
- `RefreshTokenCookieWriter`: shared `HttpOnly` + `Secure` + `SameSite=Strict` cookie logic, scoped to `/api/auth`.
- `AuthResponse` / `RegisterResponse` (API-layer only): the wire format never includes the raw refresh token - it only ever travels in the cookie.
- `ErrorMapping` updated for the three new `AuthErrors`.
- Full DI wiring in `Program.cs`, new `RefreshTokenSettings` section in `appsettings.json`.

Bugs caught during review before this build was considered done: `internal` instead of `public` on three separate classes (repeated pattern - a good one to internalize), a duplicated-vs-single-source-of-truth expiry bug in `JwtTokenGenerator`, `.AddMinutes` instead of `.AddDays` in `RefreshTokenGenerator`, a missing `IsFailure` check on `Refresh` that would have let a null-reference exception mask the theft-detection response, a cookie path mismatch that silently broke logout, and malformed JSON in `appsettings.json` that would have crashed the app on startup.

---

# Concepts Learned (so far)

- Why a stateless JWT can't be revoked, and why that's the actual problem refresh tokens solve (not "expiration clarity").
- The security/UX tradeoff behind access-token lifetime, and why refresh tokens split that into two tiers of trust.
- Refresh token rotation and reuse (replay) detection - why a stolen-and-replayed rotated-out token should revoke the whole session family, not just itself.
- Why refresh tokens are hashed before storage (same idea as password hashing, different algorithm - SHA-256, not BCrypt, because the token is high-entropy already).
- HttpOnly + Secure + SameSite cookies, and the XSS-vs-CSRF tradeoff they sit in.
- Dependency Inversion in practice: Application layer compiles and is fully reasoned about before any concrete Infrastructure/Persistence implementation exists.

---

# Interview Questions Unlocked (so far)

- Why can't you revoke a JWT once it's issued?
- Why split into a short-lived access token + long-lived refresh token instead of one long-lived token?
- What is refresh token rotation, and what attack does it defend against?
- If a rotated-out refresh token is replayed, what should the server do, and why revoke the *whole* session rather than just that token?
- Why hash a refresh token with SHA-256 instead of BCrypt, when passwords use BCrypt?
- Why does `HttpOnly` protect against XSS but not CSRF, and what closes that gap?

---

# Technical Debt (Intentional)

- **No email verification before auto-login on Register.** Register now issues a full session (access + refresh token) immediately after account creation, with no proof the email address belongs to the requester. Acceptable for now since the app has no email-verification flow at all yet; revisit if/when one is added - at that point Register should likely stop auto-issuing a session until the email is confirmed.
- **`JwtTokenGenerator` used `DateTime.Now` instead of `DateTime.UtcNow`** for token expiry (pre-existing, from Sprint 1). Being fixed as part of this sprint's Infrastructure work, since we're already touching that exact line to change its return type.
- **No rate limiting on `/login` or `/refresh`.** Both can be hammered by an attacker right now. Acceptable to defer — revisit once the Ticket domain work is further along.
- **No protection against concurrent `/refresh` calls with the same token (race condition).** Two simultaneous requests could both pass the `IsRevoked` check before either commits its rotation. Real interview-relevant gap, not fixed yet — flagged deliberately rather than silently. Fix would likely be a DB-level unique constraint or optimistic concurrency check on the token row, so the losing request fails and can be treated as a reuse signal instead of silently succeeding.
- **Unit tests done, integration tests deliberately deferred.** All four Application-layer handlers (Register, Login, Refresh, Logout) now have Moq-based unit tests covering success paths, failure paths, and behavior verification (e.g. reuse detection never issuing a session, logout being idempotent). The planned Testcontainers-based integration test project (real SQL Server in Docker, full HTTP pipeline) was scoped out for now — a deliberate time-boxing decision given a 3-month interview timeline, not an oversight. The manual sanity test already proved the same end-to-end behavior once; the conceptual understanding of why integration tests matter (real SQL constraints, catching DI/wiring bugs unit tests structurally cannot) is solid even without the implementation. Revisit if time allows after the Ticket domain is built out.

---

# Future Improvements

- "Log out of all devices" (revoke every session for a user, not just one).
- Surface active sessions/devices to the user (list + individually revoke).
- Unit of Work abstraction to replace the current per-repository `SaveChangesAsync` pattern.
