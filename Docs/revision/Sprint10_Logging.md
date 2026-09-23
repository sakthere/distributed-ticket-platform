# Sprint 10 - Structured Logging with Correlation IDs

## Milestone

The API now has real, structured, correlatable logging instead of the ASP.NET Core console default. One request in, one correlation id out, every log line for that request - across middleware, framework noise-suppression, and the one Application-layer handler wired up so far - carrying it.

```
Request in
    |
    v
CorrelationIdMiddleware   -> resolves/generates id, sets TraceIdentifier,
    |                         pushes {CorrelationId} into Serilog's LogContext
    v
ExceptionHandlingMiddleware -> catches, logs (with CorrelationId), returns ProblemDetails
    |                          (does NOT rethrow)
    v
UseSerilogRequestLogging   -> one structured summary line per request
    |                         (Verbose for successful /health/* polls)
    v
...normal pipeline (auth, controllers, handlers)...
    |
    v
LoginCommandHandler         -> ILogger<T>.LogWarning on invalid-credential paths
                               (email only, never the password)
```

---

# Progress Log

## Why this module, and why now

Every prior Sprint recap has been reconstructed from memory of what changed, and every bug report from testing so far has come from the user manually reading a stack trace or a `curl` response body. That doesn't scale past a toy project, and it's the literal first thing that breaks in a real production incident: "what actually happened, in what order, for this one request" is exactly the question logs answer and stack traces don't (a stack trace shows the fatal error; it doesn't show the three log lines that would explain *why* the code got there).

## Serilog wiring

- Replaced ASP.NET Core's built-in logging configuration (the `Logging` section in `appsettings.json`) with Serilog, configured entirely from a new `Serilog` section via `ReadFrom.Configuration`. Two sinks in the base config: `Console` (compact JSON - `Serilog.Formatting.Compact.CompactJsonFormatter` - the shape a log aggregator like Seq/ELK/Application Insights expects to ingest) and a rolling `File` sink (`logs/log-.txt`, daily rolling, 14-day retention) for local inspection without needing an aggregator running.
- `appsettings.Development.json` overrides just the Console sink's `Args` with a human-readable `outputTemplate` instead of compact JSON - because a developer staring at a terminal wants `[14:32:07 WRN] (a1b2c3d4) Login failed for user@example.com: incorrect password`, not a JSON blob, but a production log pipeline wants the JSON blob and nothing else. This is a real, non-obvious `Microsoft.Extensions.Configuration` behavior worth knowing: JSON arrays merge *by index* across layered config sources, so Development's `WriteTo[0]` (Console) overlays base's `WriteTo[0]` and only changes the `Args` given, while base's `WriteTo[1]` (File) is untouched and still applies in Development too - it's not "replace the whole array," it's "merge object-by-object at each index."
- **Two-stage initialization** (`Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();` before `WebApplication.CreateBuilder`, replaced by the fully configured logger once `builder.Configuration` exists). This is Serilog's own recommended pattern, not something invented here: if `builder.Host.UseSerilog(...)` itself fails to construct - a malformed `Serilog` config section, for instance - there needs to be *some* logger already active to report that failure, since the "real" logger depends on the configuration that just failed to parse.
- `app.Run()` wrapped in `try/Log.Fatal/finally Log.CloseAndFlush()` - the other half of the standard Serilog.AspNetCore bootstrapping idiom. `CloseAndFlush` matters specifically for the File sink, which buffers; without it, the last handful of log lines before a shutdown (graceful or crashed) can be silently lost.

## Correlation IDs

- `CorrelationIdMiddleware`, first in the pipeline. Resolves one id per request - honors an inbound `X-Correlation-Id` header (capped at 128 characters; oversized or missing values fall back to a fresh GUID) so a request can eventually be traced across a service boundary, otherwise generates one.
- **Deliberately did not invent a second id.** ASP.NET Core already assigns `HttpContext.TraceIdentifier` per request, and `ExceptionHandlingMiddleware` (built in an earlier sprint) already surfaces it inside `ProblemDetails.Extensions["traceId"]`. Overwriting `TraceIdentifier` with the resolved correlation id - instead of introducing a separately-named value - means the error response body, the response header, and every Serilog log line for the request all agree on one value. Two different "request id" concepts existing side by side would be a real, avoidable maintainability trap for the first person who has to explain to an on-call engineer which one to search logs for.
- The id is pushed into Serilog's `LogContext` for the duration of the request (`using (LogContext.PushProperty("CorrelationId", correlationId))`), which is what makes it show up on every subsequent log line - including ones written deep inside a handler - without every log call site having to pass it explicitly.
- Why cap the inbound header at 128 characters rather than trust it outright: it's untrusted client input that ends up embedded in every log line and the response for the whole request. An unbounded value is a crude log-storage-bloat vector. A newline embedded in it, by contrast, isn't the classic "log injection" risk it would be with naive string-concatenated logging - Serilog's message-template logging keeps it as one structured property value rather than splicing it into a raw text line, so it can't forge fake extra log entries. Worth knowing *why* that specific risk doesn't apply here, rather than assuming structured logging is a blanket fix for untrusted input.

## Request logging and noise control

- `app.UseSerilogRequestLogging(...)` replaces manual "request started"/"request finished" logging with one structured summary line per request (method, path, status code, elapsed time) - for free, from a single line of Program.cs.
- **Placement is deliberate, not arbitrary.** `CorrelationIdMiddleware` → `ExceptionHandlingMiddleware` → `UseSerilogRequestLogging` → the rest of the pipeline. `ExceptionHandlingMiddleware` catches and logs an unhandled exception itself and does not rethrow; `UseSerilogRequestLogging`, if placed *before* it instead, would see the same exception propagate through its own catch-log-rethrow behavior and log it a second time - once as its own summary line, once again inside `ExceptionHandlingMiddleware`. Same exception, two log entries, for no benefit. Running it after exception handling means it always sees a completed response and only ever logs one line per request, no matter what happened inside.
- Custom `GetLevel`: successful polls of `/health/live` and `/health/ready` are logged at `Verbose` (effectively silent given the `Information` minimum level configured), while a failing request anywhere - including a failing health check - stays at `Error`. Left unfiltered, an orchestrator polling every few seconds would bury every log file in "GET /health/ready responded 200" noise within hours.

## The one illustrative Application-layer log call

- `LoginCommandHandler` takes `ILogger<LoginCommandHandler>` as a constructor dependency and logs a `Warning` on both invalid-credential paths - "no account with this email" and "incorrect password" - as two internally distinct messages, even though the HTTP response returned to the caller stays the single generic `AuthErrors.InvalidCredentails` for both. The response must not leak which half of the pair was wrong (that's an existing decision, not new this sprint); the log doesn't have that constraint, and distinguishing the two internally is what would let someone later notice "500 failed logins against this one email in a minute" as a brute-force signal versus "a lot of typos happening globally."
- **Deliberately scoped to one handler, not retrofitted across the whole codebase.** Adding `ILogger<T>` and meaningful log statements to all ten existing handlers in this same sprint would be a large, low-value diff with no specific incident or story driving *which* messages actually matter yet - exactly the kind of scope creep the Working Agreement asks to avoid. This handler exists to establish the pattern and prove it doesn't violate Clean Architecture; the natural point to add a log line to some other handler is when a real need for one shows up (debugging a specific reported issue, or a security-relevant event like Assign/Delete), not as a mechanical sweep.
- `Microsoft.Extensions.Logging.Abstractions` was added as a package reference to the **Application** project (not `Microsoft.Extensions.Logging`, and definitely not Serilog). This is the same dependency-direction question as `IUnitOfWork`: Application depends on an abstraction it doesn't own, but is provider-agnostic and framework-agnostic, never on the concrete thing (Serilog) that fulfills it. Serilog is wired in exactly once, at the API layer's composition root.

---

# Concepts Learned

- **A correlation id and ASP.NET Core's `TraceIdentifier` solve the same problem - don't build a second one.** The moment you notice a new concept and an existing framework concept are answering the same question, the right move is usually to unify them, not to let both exist "just in case."
- **Structured logging (message templates with parameters) is safer against log injection than naive string concatenation** - not because untrusted input magically becomes safe, but because a single property value can't be mistaken for a delimiter between log entries the way a raw concatenated string with an embedded newline could be.
- **Middleware order changes what gets logged, not just what gets executed.** Whether `UseSerilogRequestLogging` sits before or after exception handling is the difference between one clean log line per request and duplicate, noisier logs for every unhandled exception.
- **A health check endpoint being polled every few seconds is a logging-volume problem, distinct from whether the health check itself is a good idea.** The fix isn't to stop checking health or to stop logging requests in general - it's to give that one category of request its own log level.
- **Config layering in .NET merges JSON arrays by index, not by replacing the whole array.** `appsettings.{Environment}.json` overriding one property inside `WriteTo[0]` doesn't remove `WriteTo[1]` from the base file - both apply. Easy to get a surprising "why is X still happening in Development" result if this isn't understood.

---

# Interview Questions Unlocked

- Why replace `HttpContext.TraceIdentifier` instead of introducing a separate `X-Correlation-Id` value tracked independently? What would justify having two different ids instead of one?
- Walk through why `UseSerilogRequestLogging` needs to run *after* exception-handling middleware, not before - what actually goes wrong if the order is flipped?
- Why does adding `ILogger<T>` to an Application-layer handler not violate Clean Architecture's "Application must not depend on infrastructure" rule, when Serilog itself very much would?
- You have a mocked `ILogger<T>` in a unit test. Why can't you `Verify(l => l.LogWarning(...))` directly, and what do you have to verify instead?
- What's the actual difference between a liveness check being noisy in logs and a liveness check being wrong? Why is filtering log *volume* the right fix here rather than checking health less often?
- Why is capping the length of an inbound `X-Correlation-Id` header worth doing, given that Serilog's structured logging already defuses the classic log-injection concern for it?

---

# Technical Debt (Intentional)

- Only `LoginCommandHandler` has been instrumented with meaningful `ILogger` calls. The other nine handlers (Register, Refresh, Logout, and all six Ticket handlers) have zero logging beyond what `ExceptionHandlingMiddleware` and `UseSerilogRequestLogging` already give them for free. Intentional - add logging to a specific handler when a specific need (a debugging session, a security-relevant event) actually calls for it, not as a mechanical sweep with no driving story.
- No centralized log aggregation sink (Seq, ELK, Application Insights, CloudWatch) is wired up - Console (compact JSON) and a local rolling File are the only two sinks. Fine for local development and even a single-instance deployment; becomes a real gap the moment there's more than one instance and "grep the file on the box" stops being a viable way to investigate an incident.
- No log-based alerting exists yet (e.g., "page someone if N failed logins for one account in five minutes"). The Warning-level log lines from `LoginCommandHandler` are the raw material such alerting would consume, but nothing consumes them yet.
- `CorrelationIdMiddleware` trusts an inbound `X-Correlation-Id` header up to a length cap, but a malicious caller could still forge a plausible-looking id belonging to a different, legitimate request to make log lines from an unrelated request appear to share a correlation id. Not a real risk today (there's no second service for the id to be forwarded to or from yet), but revisit if this API ever calls or is called by another service where a forged correlation id could be used to obscure or mislead an investigation.

---

# Future Improvements

- Phase 3's remaining item is API Versioning.
- The moment a second real logging need shows up in any other handler, add it there rather than doing a mechanical sweep of the remaining nine.
- If/when this system is deployed anywhere with more than one running instance, replace or supplement the File sink with a real aggregator - this is the same "revisit when it blocks future development" trigger the Working Agreement uses elsewhere, not a scheduled task with its own timeline yet.
