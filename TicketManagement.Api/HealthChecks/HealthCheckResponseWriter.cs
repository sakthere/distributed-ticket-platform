using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TicketManagement.Api.HealthChecks
{
    // ASP.NET Core's default health check response is just the plain-text status
    // ("Healthy"/"Unhealthy"). That's not enough in production: when a check fails
    // you need to know WHICH dependency failed and why, without grepping logs first.
    // This writer produces a small JSON payload with per-check status, description
    // and duration - the shape most uptime dashboards / on-call runbooks expect.
    public static class HealthCheckResponseWriter
    {
        public static Task WriteResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var payload = new
            {
                status = report.Status.ToString(),
                totalDurationMs = report.TotalDuration.TotalMilliseconds,
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    durationMs = entry.Value.Duration.TotalMilliseconds
                })
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
