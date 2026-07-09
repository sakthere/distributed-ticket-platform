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