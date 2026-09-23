using Serilog.Context;

namespace TicketManagement.Api.Middleware
{
    // Every log line written during a request needs a shared id so an on-call
    // engineer can pull ONE request's worth of log lines out of a sea of
    // interleaved concurrent requests. ASP.NET Core already generates one such
    // id per request - HttpContext.TraceIdentifier - and ExceptionHandlingMiddleware
    // already surfaces it to API callers inside ProblemDetails.
    //
    // Rather than inventing a second, competing "correlation id" that lives
    // alongside TraceIdentifier, this middleware unifies them: it resolves a
    // single id (honoring one supplied by an upstream caller, so a request can be
    // traced across service boundaries later), overwrites TraceIdentifier with
    // it, and pushes it into Serilog's LogContext so every log line for this
    // request - from any layer - carries it automatically. One id, one name,
    // everywhere: log lines, error responses, and the response header all agree.
    public class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-Id";

        // An inbound header is untrusted input. A caller could send an
        // absurdly long value (log storage bloat, a crude cost/DoS vector once
        // it's copied into every log line for the request) - cap it rather
        // than trusting it blindly. Structured logging (Serilog writes this as
        // one property value via a message template, never by concatenating
        // strings into a log line) means an embedded newline can't forge extra
        // fake log entries the way it could with naive string-built logging,
        // but an unbounded length is still worth rejecting outright.
        private const int MaxHeaderLength = 128;

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = ResolveCorrelationId(context);

            context.TraceIdentifier = correlationId;
            context.Response.Headers[HeaderName] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        private static string ResolveCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(HeaderName, out var incoming))
            {
                var value = incoming.ToString();
                if (!string.IsNullOrWhiteSpace(value) && value.Length <= MaxHeaderLength)
                {
                    return value;
                }
            }

            return Guid.NewGuid().ToString();
        }
    }
}
