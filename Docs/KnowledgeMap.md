# Knowledge Map

> This document is the engineering knowledge accumulated while building the Ticket Management System.
>
> It is NOT project documentation.
>
> It is my interview revision notebook.
>
> Every topic is categorized into:
>
> - 🧠 Must Memorize
> - 🟡 Must Understand
> - 📚 Lookup Knowledge
> - 🏗 Production Notes
> - 🎯 Interview Questions

---

# 1. Clean Architecture

## 🧠 Must Memorize

- Domain
- Application
- Infrastructure
- Persistence
- API

Dependency Flow

```
API
 ↓
Application
 ↓
Domain

Infrastructure
 ↓

Persistence
 ↓
```

Domain never depends on Infrastructure.

---

## 🟡 Must Understand

Why we separate layers.

- Testability
- Maintainability
- Loose Coupling
- Dependency Inversion

---

## 📚 Lookup

Project reference syntax.

---

## 🏗 Production

Large teams can work independently on separate layers.

---

## 🎯 Interview

Why Clean Architecture?

Why not put everything inside Controllers?

---

# 2. Vertical Slice Architecture

## 🧠 Must Memorize

Organize by Feature.

NOT by Layer.

```
Authentication
    Register
        Command
        Handler
        Validator
        Result
```

---

## 🟡 Must Understand

A feature owns everything required to complete that use case.

Controllers become thin.

---

## 📚 Lookup

Folder structure.

---

## 🏗 Production

Much easier to evolve than large Service folders.

---

## 🎯 Interview

Difference between Clean Architecture and Vertical Slice?

---

# 3. CQRS

## 🧠 Must Memorize

Command

Writes.

Query

Reads.

Commands change state.

Queries never change state.

---

## 🟡 Must Understand

Separating read/write reduces coupling and keeps business logic focused.

---

## 📚 Lookup

MediatR syntax.

---

## 🏗 Production

Large systems often split Read DB and Write DB.

---

## 🎯 Interview

When should CQRS NOT be used?

---

# 4. Repository Pattern

## 🧠 Must Memorize

Repository abstracts persistence.

Application never knows EF Core.

---

## 🟡 Must Understand

Repositories expose business operations.

NOT IQueryable.

---

## 📚 Lookup

EF Core APIs.

---

## 🏗 Production

Repositories should not become God classes.

---

## 🎯 Interview

Why not expose IQueryable?

---

# 5. Dependency Injection

## 🧠 Must Memorize

Transient

Scoped

Singleton

---

Scoped

One instance per HTTP request.

---

## 🟡 Must Understand

Depend on abstractions.

Container creates implementations.

---

## 📚 Lookup

builder.Services syntax.

---

## 🏗 Production

Wrong lifetime causes bugs.

Never inject Scoped into Singleton.

---

## 🎯 Interview

Difference between Scoped and Singleton?

---

# 6. FluentValidation

## 🧠 Must Memorize

Validation is input validation.

Not business validation.

Runs before Controller execution.

---

## 🟡 Must Understand

Validation is a cross-cutting concern.

Controllers shouldn't call validators.

---

## 📚 Lookup

Registration syntax.

Assembly scanning.

---

## 🏗 Production

Never duplicate validation inside Handlers.

---

## 🎯 Interview

Difference between FluentValidation and DataAnnotations?

---

# 7. JWT

## 🧠 Must Memorize

JWT

Header

Payload

Signature

Claims

Issuer

Audience

Expiry

Bearer Token

Authentication

Authorization

---

## 🟡 Must Understand

JWT is

NOT encrypted.

It is

Base64 encoded

+

Digitally Signed.

HMAC SHA256 protects integrity.

Claims represent the authenticated user.

---

## 📚 Lookup

TokenValidationParameters

Swagger configuration

JwtBearer configuration

---

## 🏗 Production

Never store secrets in JWT.

Use environment variables or Key Vault.

Implement Refresh Tokens.

Support Key Rotation.

---

## 🎯 Interview

JWT vs Sessions

Authentication vs Authorization

Why JWT is stateless?

---

# 8. Password Hashing

## 🧠 Must Memorize

Never store passwords.

Hash them.

Use BCrypt.

---

## 🟡 Must Understand

Hash

≠

Encryption.

Hashing is one-way.

Verification compares password against stored hash.

---

## 📚 Lookup

BCrypt API.

---

## 🏗 Production

Never invent your own hashing algorithm.

---

## 🎯 Interview

Why BCrypt?

---

# 9. Authentication Pipeline

## 🧠 Must Memorize

```
Request

↓

Authentication

↓

Authorization

↓

Controller
```

Order matters.

---

## 🟡 Must Understand

Authentication answers

Who are you?

Authorization answers

What can you do?

---

## 📚 Lookup

JWT middleware registration.

---

## 🏗 Production

Always authenticate before authorizing.

---

## 🎯 Interview

Why UseAuthentication() before UseAuthorization()?

---

# 10. Swagger

## 🧠 Must Memorize

Swagger documents APIs.

---

## 🟡 Must Understand

Swagger doesn't know authentication.

We describe the Security Scheme.

---

## 📚 Lookup

Bearer Security Definition configuration.

---

## 🏗 Production

Document every endpoint.

---

## 🎯 Interview

Why configure Swagger for JWT?

---

# 11. HTTP Status Codes

## 🧠 Must Memorize

200 OK

201 Created

400 Bad Request

401 Unauthorized

403 Forbidden

404 Not Found

409 Conflict

500 Internal Server Error

---

## 🟡 Must Understand

Choose the status code based on HTTP semantics.

---

## 📚 Lookup

ProblemDetails configuration.

---

## 🏗 Production

Always return consistent error responses.

---

## 🎯 Interview

401 vs 403?

---

# 12. SOLID

## 🧠 Must Memorize

S

Single Responsibility

O

Open Closed

L

Liskov

I

Interface Segregation

D

Dependency Inversion

---

## 🟡 Must Understand

Every class should naturally follow SOLID.

Don't force patterns.

---

## 📚 Lookup

Examples.

---

## 🏗 Production

Review every PR through SOLID.

---

## 🎯 Interview

Real-world examples of each principle.

---

# 13. Design Patterns

## 🧠 Must Memorize

Repository

Dependency Injection

CQRS

Vertical Slice

---

## 🟡 Must Understand

Patterns solve recurring problems.

Never use a pattern because a tutorial did.

---

## 📚 Lookup

Pattern implementations.

---

## 🏗 Production

Introduce patterns only when justified.

---

## 🎯 Interview

Repository vs Unit of Work.

Factory vs Builder.

Strategy vs State.

---

# Upcoming Topics

- Global Exception Middleware
- Result<T>
- MediatR
- Unit Of Work
- AutoMapper
- Caching
- Redis
- Background Jobs
- RabbitMQ
- Docker
- Serilog
- Health Checks
- OpenTelemetry
- API Versioning
- Rate Limiting
- Polly
- Outbox Pattern
- Domain Events
- Microservices
- Kubernetes
- Azure


---

# 14. Middleware

## 🧠 Must Memorize

### Middleware

Middleware is a component that participates in the ASP.NET Core HTTP request pipeline.

Every incoming request passes through the middleware pipeline before reaching the controller.

Every outgoing response travels back through the same middleware pipeline.

---

### Request Pipeline

```text
Request
    │
    ▼
Exception Middleware
    │
    ▼
Authentication
    │
    ▼
Authorization
    │
    ▼
Controller
    │
    ▼
Handler
    │
    ▼
Repository
    │
    ▼
SQL Server
    │
    ▲
Repository
    ▲
Handler
    ▲
Controller
    ▲
Authorization
    ▲
Authentication
    ▲
Exception Middleware
    ▲
Response
```

---

### Middleware Execution

Middleware executes code:

- Before calling the next middleware
- After the next middleware finishes

```csharp
await _next(context);

// Execution returns here
```

---

### Important Order

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
```

Authentication **must** execute before Authorization.

Exception Handling should be the first middleware so it can catch failures from everything downstream.

---

## 🟡 Must Understand

### Why Middleware Exists

Middleware provides a way to implement cross-cutting concerns once for the entire application.

Examples include:

- Exception Handling
- Authentication
- Authorization
- Logging
- Rate Limiting
- CORS
- Compression
- Caching

Instead of duplicating logic inside every controller.

---

### Why RequestDelegate?

`RequestDelegate` represents the **next component** in the HTTP request pipeline.

Calling

```csharp
await _next(context);
```

passes execution to the next middleware.

When that middleware finishes, execution returns to the current middleware.

---

### Why Middleware isn't registered with AddScoped()

Application services are resolved because another class requests them through Dependency Injection.

Middleware is different.

Middleware becomes part of the HTTP request pipeline using:

```csharp
app.UseMiddleware<T>();
```

ASP.NET Core constructs the middleware and resolves its constructor dependencies automatically.

---

### Why Global Exception Middleware?

Instead of writing:

```csharp
try
{
}
catch
{
}
```

inside every controller,

we centralize exception handling.

Benefits:

- Consistent responses
- Consistent logging
- Better security
- Easier maintenance

---

### Why ProblemDetails?

ProblemDetails follows RFC 7807, the standard error response format for HTTP APIs.

Benefits:

- Consistent structure
- Standardized clients
- Easier debugging
- Supported by ASP.NET Core

---

### Why not return Exception.Message?

Exception messages often expose:

- SQL queries
- Stack traces
- Internal class names
- Server details

Clients should receive a generic response.

Developers should inspect the logs.

**Logs are for developers.**

**Responses are for clients.**

---

## 📚 Lookup

- Middleware registration syntax
- UseMiddleware<T>()
- WriteAsJsonAsync()
- ProblemDetails
- RequestDelegate
- HttpContext APIs

---

## 🏗 Production Notes

- Keep middleware focused on one responsibility.
- Middleware should remain small (generally under 100 lines).
- Use structured logging instead of string concatenation.
- Include TraceId or CorrelationId in every error response.
- Never leak internal exception details.
- Return RFC7807 ProblemDetails for API errors.

Future improvements:

- Exception-to-HTTP status mapping
- Correlation IDs
- Request Logging Middleware
- Serilog integration

---

## 🎯 Interview Questions

### Basic

- What is middleware?
- How does middleware differ from filters?
- What is RequestDelegate?

### Intermediate

- Why does middleware execute both before and after the next middleware?
- Why should Exception Middleware be registered first?
- Why must Authentication execute before Authorization?
- Why doesn't middleware use AddScoped()?

### Advanced

- Explain the ASP.NET Core request pipeline.
- How would you build custom middleware?
- How would you standardize error responses across a distributed system?
- How would you add correlation IDs to every request?

---

## 🔗 Related Concepts

- Authentication
- Authorization
- Dependency Injection
- HTTP Request Pipeline
- Chain of Responsibility Pattern
- ProblemDetails (RFC7807)
- Structured Logging