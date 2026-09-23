# Sprint 11 - API Versioning

## Milestone

Phase 3 (Engineering Improvements) is now complete. Every route in the API is versioned via a URL segment, Swagger documents each version separately, and two real bugs that versioning would otherwise have silently introduced were caught and fixed before they shipped.

```
Before                          After
------------------------------   -------------------------------------------
POST /api/auth/login             POST /api/v1/auth/login
GET  /api/tickets/{id}           GET  /api/v1/tickets/{id}

Swagger: one undifferentiated    Swagger: one document per discovered
         document                        version (v1 today, v2 later - zero
                                          extra code needed when it arrives)
```

---

# Progress Log

## Why version at all, and why now

Every endpoint built so far has one implicit contract: whatever shape `TicketResponse`/`AuthResponse`/etc. happen to be today. The moment any of those needs a breaking change - a renamed field, a different status code, a restructured response - every existing caller (a frontend, a mobile client, a third-party integration) breaks at the same instant the new code deploys, with no way to migrate on their own schedule. Versioning is what turns "everyone breaks right now" into "v1 keeps working, v2 exists side by side, callers migrate when they're ready." Doing it now, while there's only one version and no real consumers yet, means it's a clean, low-risk change - retrofitting versioning onto an API with active clients and years of undocumented URL assumptions is a much bigger, riskier project.

## Versioning scheme chosen

- **URL segment versioning** - `api/v{version:apiVersion}/tickets`, `api/v{version:apiVersion}/auth` - via `Asp.Versioning.Mvc` (the actively maintained continuation of the archived `Microsoft.AspNetCore.Mvc.Versioning`) and `Asp.Versioning.Mvc.ApiExplorer` for Swagger integration.
- Considered and rejected:
  - **Header-based** (a custom `X-Api-Version` header, or media-type versioning via `Accept: application/vnd.company.v1+json`) - arguably "more RESTful" in the sense that a resource's URL shouldn't change just because its representation version does, but far less discoverable: can't be tested by pasting a URL into a browser, every curl call needs an extra header remembered, and some caches/CDNs need explicit `Vary` configuration to avoid serving the wrong version's cached response.
  - **Query string** (`?api-version=1.0`) - simple, but this API's List/Search Tickets endpoint already uses the query string heavily for paging/filtering/sorting (`?page=&status=&sortBy=`); mixing a routing concern into the same parameter space as filter state is the messiest of the three options here specifically.
  - URL segment won on discoverability and on not colliding with existing query-string usage - the concrete, present tradeoffs mattered more here than the purist "URLs are forever" argument against it, given this project's Swagger-first, demo-and-interview-driven usage pattern.
- `DefaultApiVersion(1.0)` + `AssumeDefaultVersionWhenUnspecified = true`: an unversioned request still resolves to v1 rather than failing outright - a deliberate soft landing for any client that hasn't added the version segment yet, not a permanent guarantee that unversioned requests will always work.
- `ReportApiVersions = true` - adds `api-supported-versions`/`api-deprecated-versions` response headers automatically. A client library can inspect these to know what's available without needing an out-of-band changelog. Free, from one config flag - a good example of using what the library already gives you instead of hand-rolling the same thing.

## Swagger integration

- `ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>` (`TicketManagement.Api/Swagger/`) registers one Swagger document per version reported by `IApiVersionDescriptionProvider`, rather than one hardcoded document. `AddSwaggerGen`'s own inline lambda can't take a constructor dependency - `IApiVersionDescriptionProvider` only exists once `AddApiVersioning().AddApiExplorer()` have registered it, and `IConfigureOptions<T>` is the standard way to get real constructor injection into an options type's configuration instead of an anonymous lambda. This is the same pattern the `aspnet-api-versioning` project's own Swashbuckle sample uses - not something invented here.
- `app.UseSwaggerUI` now loops over `ApiVersionDescriptions` and registers one `SwaggerEndpoint` per version. Adding a `[ApiVersion("2.0")]` controller later needs no changes to this loop or to `ConfigureSwaggerOptions` - a second document just appears.

## Two real bugs caught by the changeover (not invented to pad this sprint)

- **The refresh-token cookie's `Path` would have silently broken refresh/logout.** `RefreshTokenCookieWriter` scoped the `HttpOnly` refresh-token cookie to `Path = "/api/auth"` - a browser only attaches a cookie to a request whose path starts with that exact prefix. Moving the route to `api/v{version:apiVersion}/auth` means the actual request path is now `/api/v1/auth/refresh`, which does **not** start with `/api/auth` (`v1` sits in between). Left unfixed, this wouldn't throw an exception anywhere - the browser would just quietly stop sending the cookie, and `Refresh`/`Logout` would both start returning `401 Unauthorized` as if the client had no session. Fixed by updating the cookie's `Path` to `/api/v1/auth` to match. This is exactly the kind of bug that unit tests (which mock everything below the controller) cannot catch - it only exists at the real HTTP/cookie layer, another concrete argument for the integration-test gap flagged in Sprints 8-10.
- **Hardcoded `Location` header literals in `Created(...)` calls went stale.** `AuthController.Register` and `TicketController.Create` both build their `201 Created` `Location` header from a hand-written string literal (`$"api/auth/{id}"`, `$"api/tickets/{id}"`) rather than `Url.Action`/`CreatedAtAction`. Both needed manual updates to `api/v1/...` to stay valid - a `Location` header pointing at a URL that no longer resolves is a real, if minor, correctness bug (a well-behaved client following that header would get a 404). Fixed in place rather than converting to `CreatedAtAction`, since API-versioning route-value resolution for `Url.Action` adds its own complexity that isn't warranted just to fix this; flagged as tech debt below instead.

## Cleanup

- Removed `TestController` (`GET /api/test`, `GET /api/test/admin-only`) - Phase 1 scaffolding used to manually verify JWT authentication/role checks before real Auth and Ticket endpoints (and their actual test suites) existed. It had no production purpose, and versioning it would have forced a decision (which version does throwaway test scaffolding belong to?) for code that should have been deleted once the real endpoints existed. Historical Sprint 3 notes still describe what it was for at the time; the file itself is gone.

---

# Concepts Learned

- **Versioning is cheapest before you need it and most expensive after.** With zero real external consumers today, this was a routing change and two follow-on fixes. The same change against an API with live clients and undocumented URL assumptions would be a migration project with a deprecation timeline, not an afternoon.
- **A cookie's `Path` is a literal string prefix match, not a semantic "belongs to this feature" scope.** Renaming a route without checking what else assumes that literal string (a cookie path, a hardcoded `Location` header, external documentation, a frontend's hardcoded base URL) is how a routing change becomes a silent runtime bug instead of a compile error - nothing here would have failed to build.
- **`IConfigureOptions<T>` exists specifically for the case where an options type's configuration needs a service that isn't available at the point `AddXyz(options => ...)` is called.** This shows up constantly in real ASP.NET Core code (versioned Swagger being the single most common example) and is worth recognizing as a pattern, not memorizing as a one-off trick.
- **A soft-landing default (`AssumeDefaultVersionWhenUnspecified`) is a deliberate compatibility decision, not a loophole.** It exists to make a rollout less disruptive, and is exactly the kind of thing that should have an explicit expiration point in mind (a written decision to eventually require an explicit version), not be left permanently ambiguous.

---

# Interview Questions Unlocked

- Walk through the three common API versioning schemes (URL segment, header, query string) and their real tradeoffs - not the textbook list, the concrete ones that applied to this specific API.
- Why does `AddSwaggerGen(options => ...)` need a separate `IConfigureOptions<T>` class instead of just injecting a service into that lambda?
- You renamed a route. What are the categories of things elsewhere in a codebase that could silently break as a result, beyond "does it still compile"?
- What does `Path` actually mean on an HTTP cookie, and how is that different from "this cookie is scoped to the auth feature"?
- What's the tradeoff of `AssumeDefaultVersionWhenUnspecified = true` - what does it buy you, and what risk does it quietly introduce if left on indefinitely?
- Why fix the hardcoded `Location` header literals in place rather than switching to `CreatedAtAction` while already touching this code?

---

# Technical Debt (Intentional)

- `AuthController.Register` and `TicketController.Create` still build their `201 Created` `Location` header from hand-written string literals instead of `Url.Action`/`CreatedAtAction`. This is pre-existing tech debt (not introduced this sprint) that versioning made visible and forced a manual fix for - the next route rename will hit the exact same issue again unless this is addressed. Not converted now because `CreatedAtAction` under API-versioned routing needs the version supplied as an explicit route value, which is a real but separate piece of complexity; revisit if a third route rename makes the pattern's cost too obvious to ignore again.
- The refresh-token cookie's `Path` is now coupled to the literal string `"/api/v1/auth"` - correct for today (there's exactly one version), but the day a `v2.0` `AuthController` exists, this needs a deliberate decision: exempt refresh/logout from versioning entirely (a session mechanic isn't really a "resource representation" that should vary by version), or re-scope the cookie's `Path` more broadly (e.g. `/api`). Recorded here rather than solved now, since there's no second version yet to decide between those options against.
- `AssumeDefaultVersionWhenUnspecified = true` has no expiration or deprecation plan attached - it's a permanent soft landing right now, not a temporary one. Revisit once there's a real reason to require every caller to specify a version explicitly (e.g. once `v2.0` exists and "no version specified" needs to mean something more deliberate than "assume the oldest one").
- Still no integration test project (flagged in Sprints 8, 9, and again here) - the refresh-token cookie `Path` bug this sprint is now the third concrete, specific bug that only exists at the real HTTP layer and that unit tests mocking every dependency structurally cannot catch.

---

# Future Improvements

- Phase 3 (Engineering Improvements) is complete: Global Exception Middleware, Result Pattern, Unit of Work, Logging, Health Checks, and API Versioning are all in place.
- Phase 4 (Scalability) is next per the roadmap: Redis, Background Jobs, RabbitMQ, Email Notifications, Caching.
- The integration test project flagged repeatedly across the last several sprints keeps getting a new concrete justification every sprint - worth prioritizing before Phase 4 adds more infrastructure (Redis, message queues) that will be even harder to reason about with only mocked unit tests.
