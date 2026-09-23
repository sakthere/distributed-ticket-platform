
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using TicketManagement.Api.Swagger;
using TicketManagement.Application.Features.Authentication.Login;
using TicketManagement.Application.Features.Authentication.Register;
using TicketManagement.Application.Interfaces;
using TicketManagement.Infrastructure.Authentication.Hashing;
using TicketManagement.Infrastructure.Authentication.Jwt;
using TicketManagement.Persistence;
using TicketManagement.Persistence.Context;
using TicketManagement.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using TicketManagement.Api.Middleware;
using TicketManagement.Api.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TicketManagement.Infrastructure.Authentication.RefreshTokens;
using TicketManagement.Application.Features.Authentication.Common;
using TicketManagement.Application.Features.Authentication.RefreshToken;
using TicketManagement.Application.Features.Authentication.Logout;
using TicketManagement.Application.Features.Tickets.Assign;
using TicketManagement.Application.Features.Tickets.ChangeStatus;
using TicketManagement.Application.Features.Tickets.Create;
using TicketManagement.Application.Features.Tickets.Delete;
using TicketManagement.Application.Features.Tickets.Get;
using TicketManagement.Application.Features.Tickets.List;
using TicketManagement.Application.Features.Tickets.OverridePriority;
using TicketManagement.Application.Features.Tickets.Update;

// Two-stage Serilog initialization (the pattern Serilog.AspNetCore itself
// recommends). This "bootstrap" logger is deliberately minimal - it only
// needs to survive long enough to report a startup failure (bad config,
// DB connection string missing, DI misconfiguration) to the console before
// the host and its configuration/DI system even exist. It gets replaced
// wholesale by the fully configured logger a few lines down, once
// builder.Configuration is available to read the real "Serilog" settings from.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "TicketManagement.Api");
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer Token"
    });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            { 
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }});
});
builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();

// URL-segment versioning (api/v1/tickets, not a header or query string): of
// the three common schemes, this is the one that stays visible and testable
// with nothing more than a browser address bar or a bare curl command - no
// custom header to remember, and no collision with the query-string
// parameters List/Search Tickets already uses for paging/filtering/sorting.
// AssumeDefaultVersionWhenUnspecified + DefaultApiVersion(1.0) means existing
// callers hitting an unversioned path during any transition period still
// resolve to v1 instead of failing outright - a deliberate soft landing, not
// a permanent guarantee.
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true; // adds api-supported-versions / api-deprecated-versions response headers, for free
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";       // "1.0" -> "v1" in Swagger's grouping
    options.SubstituteApiVersionInUrl = true;  // resolves {version:apiVersion} in Swagger-displayed routes
});

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();



builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    var JwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = JwtSettings!.Issuer,
        ValidAudience = JwtSettings.Audience,

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.SecretKey))
    };
});

builder.Services.AddAuthorization();

// Two-tier health checks, mirroring how a Kubernetes-style orchestrator
// (or any load balancer) probes a service:
//   - Liveness ("is the process alive?"): no dependency checks at all. If this
//     fails, the process itself is wedged and should be restarted. Checking the
//     database here would be wrong - a slow/down DB would get a perfectly healthy
//     process killed and restarted for no reason, which doesn't fix the DB and
//     can make an outage worse (a restart storm).
//   - Readiness ("can it currently serve traffic?"): checks the database, since
//     this API can't do its job without one. If this fails, a load balancer
//     should stop routing new requests here, but the process should NOT be
//     restarted - it should recover on its own once the DB is back.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(name: "database", tags: new[] { "ready" });
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<RegisterCommandHandler>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.Configure<RefreshTokenSettings>(builder.Configuration.GetSection("RefreshTokenSettings"));

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();
builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
builder.Services.AddScoped<IAuthSessionIssuer, AuthSessionIssuer>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<CreateTicketCommandHandler>();
builder.Services.AddScoped<RefreshCommandHandler>();
builder.Services.AddScoped<LogoutCommandHandler>();
builder.Services.AddScoped<UpdateTicketCommandHandler>();
builder.Services.AddScoped<DeleteTicketCommandHandler>();
builder.Services.AddScoped<AssignTicketCommandHandler>();
builder.Services.AddScoped<GetTicketByIdQueryHandler>();
builder.Services.AddScoped<ChangeTicketStatusCommandHandler>();
builder.Services.AddScoped<OverrideTicketPriorityCommandHandler>();
builder.Services.AddScoped<GetTicketsQueryHandler>();

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var versionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in versionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
}

// Ordering matters here:
//  1. CorrelationIdMiddleware first - every log line written by anything
//     after this point, including the exception log below, needs the
//     correlation id already pushed into Serilog's LogContext.
//  2. ExceptionHandlingMiddleware next, wrapping everything downstream -
//     it converts an unhandled exception into a ProblemDetails response
//     and logs it itself, WITHOUT rethrowing.
//  3. UseSerilogRequestLogging last of the three - by running after
//     exception handling, it always sees a completed response (200, 404,
//     500, whatever) and logs exactly one summary line per request. If this
//     were placed before ExceptionHandlingMiddleware instead, its own
//     built-in catch-log-rethrow behavior would log the same exception a
//     second time, once as its summary line and again inside
//     ExceptionHandlingMiddleware - duplicate, noisier logs for zero benefit.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    // A load balancer or orchestrator polls /health/* every few seconds.
    // Logging every one of those at Information would drown out the
    // requests that actually matter. Demote successful health polls to
    // Verbose (effectively silent at the Information level configured in
    // appsettings) while keeping genuine failures - anywhere, including
    // health checks - visible at Error.
    options.GetLevel = (httpContext, elapsed, ex) =>
        ex != null
            ? LogEventLevel.Error
            : httpContext.Request.Path.StartsWithSegments("/health")
                ? LogEventLevel.Verbose
                : LogEventLevel.Information;
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false, // run zero checks - this endpoint only proves the process is up
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

try
{
    Log.Information("Starting TicketManagement.Api");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    // Serilog sinks (especially the file sink) buffer writes - flush
    // on the way out so the last few log lines aren't silently lost on
    // shutdown, whether that shutdown is graceful or a startup crash.
    Log.CloseAndFlush();
}
