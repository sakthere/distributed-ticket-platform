# Sprint 1 - User Registration Vertical Slice

## Milestone

Implement the first end-to-end feature using Vertical Slice Architecture.

```
HTTP Request
      │
      ▼
Controller
      │
      ▼
RegisterCommandHandler
      │
      ▼
UserRepository
      │
      ▼
EF Core
      │
      ▼
SQL Server
```

---

# Feature Completed

- User Registration Endpoint
- Vertical Slice Architecture
- CQRS (Command)
- Repository Pattern
- Dependency Injection
- Password Hashing
- FluentValidation
- Result Pattern (Initial)
- SQL Persistence
- Swagger Testing

---

# Concepts Learned

## Architecture

- Clean Architecture
- Vertical Slice Architecture
- CQRS Basics
- Layer Responsibilities
- Dependency Injection

---

## Backend

- EF Core Repository Pattern
- Password Hashing
- FluentValidation
- API Controllers
- Request Pipeline
- DTO vs Entity
- Business Validation
- Input Validation

---

## ASP.NET Core

- AddControllers()
- MapControllers()
- Dependency Injection Container
- MVC Request Pipeline
- Swagger Integration

---

# Interview Questions Unlocked

## Architecture

- Why Clean Architecture?
- Why Vertical Slice?
- Why CQRS?
- Difference between Commands and Queries?
- Why thin controllers?

---

## Validation

- Why FluentValidation?
- Why not DataAnnotations?
- Difference between input validation and business validation?

---

## Repository

- Why Repository Pattern?
- Why not expose IQueryable?
- Why use abstractions?

---

## Authentication

- Why hash passwords?
- Why BCrypt?
- Why not store plaintext passwords?

---

## ASP.NET Core

- Difference between AddControllers() and MapControllers()?
- What happens if MapControllers() is missing?
- How does Dependency Injection work?

---

# Design Patterns Used

| Pattern | Purpose |
|----------|----------|
| Repository | Database abstraction |
| Dependency Injection | Loose coupling |
| CQRS | Separate read/write operations |
| Vertical Slice | Organize by feature |

---

# SOLID Review

## Single Responsibility Principle

✅ RegisterHandler performs one business operation.

---

## Open Closed Principle

🟡 Future improvement through Result Pattern and MediatR.

---

## Liskov Substitution Principle

✅ Repository depends on abstractions.

---

## Interface Segregation Principle

✅ IUserRepository is small and focused.

---

## Dependency Inversion Principle

✅ Handler depends on interfaces rather than EF Core.

---

# Production Review

## Good

- Password hashing
- Repository abstraction
- Validation before business logic
- Thin controller

---

## Missing

- Logging
- Exception Middleware
- Unit Tests
- Integration Tests
- Email uniqueness constraint in SQL
- Transactions
- Unit of Work
- Rate Limiting
- Monitoring

---

# Mistakes Made

1. Started with .NET 10 Preview instead of .NET 8 LTS.

2. Initially forgot to register FluentValidation.

3. Mixed MVC validation with middleware concepts.

4. Created an AuthService before freezing the architecture.

5. Almost returned null instead of designing a Result pattern.

6. Forgot to compare generated code against the actual domain model.

---

# Lessons Learned

- Build one feature completely before starting another.
- Never build on a broken application.
- Validate assumptions with evidence.
- Business logic should remain outside controllers.
- Validation belongs before business logic.
- Don't expose EF Core outside Persistence.
- Design evolves through deliberate refactoring.

---

# Technical Debt (Intentional)

- SaveChangesAsync currently resides in Repository.
- Will migrate to Unit of Work in Sprint 3.

---

# Future Improvements

- Result<T>
- MediatR
- Unit Of Work
- Domain Events
- Factory Pattern
- Global Exception Middleware
- Logging
- Redis
- RabbitMQ

---

# Sprint Status

## Completed

- Sprint 0
- Sprint 1 - Register

---

## Next Sprint

Authentication

- Login
- JWT Generation
- Authentication Middleware
- Authorization
- Claims
- Refresh Tokens

---

# Self Revision Checklist

- [ ] Explain Vertical Slice Architecture.
- [ ] Explain CQRS.
- [ ] Explain Repository Pattern.
- [ ] Explain Dependency Injection.
- [ ] Explain FluentValidation.
- [ ] Explain AddControllers().
- [ ] Explain MapControllers().
- [ ] Explain Password Hashing.
- [ ] Explain DTO vs Entity.
- [ ] Explain Result Pattern.
- [ ] Explain Input vs Business Validation.