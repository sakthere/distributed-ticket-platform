# Project Roadmap

## Phase 1 - Authentication ✅

- [x] Register
- [x] Login
- [x] JWT
- [x] Authentication
- [x] Authorization

---

## Phase 1.5 - Testing Foundation

- [x] Unit tests (Application layer handlers — Register, Login, Refresh, Logout)
- [ ] Integration tests (API layer, real DB or Testcontainers) — deliberately deferred, see tech debt in Sprint2 recap

---

## Phase 2 - Ticket Management

- [x] Create Ticket
- [x] Update Ticket
- [x] Delete Ticket
- [x] Assign Ticket
- [x] Get Ticket (not originally scoped in this roadmap, added when the ticket CRUD loop was closed out)
- [x] Ticket Status (explicit state-machine transitions - see TicketStatusPolicy)
- [x] Ticket Priority (override by Admin/Agent after creation)
- [x] List/Search Tickets (pagination, filtering, sorting)

**Phase 2 is complete.**

---

## Phase 3 - Engineering Improvements

- [x] Global Exception Middleware (implemented alongside Phase 1 error handling - see project error-handling notes)
- [x] Result Pattern (`Result`/`Result<T>` for business failures - implemented alongside Phase 1)
- [x] Unit of Work (single `IUnitOfWork.SaveChangesAsync`, replacing per-repository `SaveChangesAsync` - see Sprint 9)
- [ ] Logging
- [x] Health Checks (`/health/live`, `/health/ready` - see Sprint 9)
- [ ] API Versioning

---

## Phase 4 - Scalability

- [ ] Redis
- [ ] Background Jobs
- [ ] RabbitMQ
- [ ] Email Notifications
- [ ] Caching

---

## Phase 5 - DevOps

- [ ] Docker
- [ ] Docker Compose
- [ ] CI/CD
- [ ] Azure Deployment

---

## Phase 6 - Distributed Systems

- [ ] Microservices
- [ ] API Gateway
- [ ] Identity Service
- [ ] Notification Service
- [ ] Observability