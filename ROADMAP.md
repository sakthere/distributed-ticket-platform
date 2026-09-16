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
- [ ] Ticket Priority (override by Admin/Agent after creation - still open, see Sprint 5 Future Improvements)
- [ ] List/Search Tickets (pagination, filtering, sorting — needed once Get's single-read model isn't enough)

---

## Phase 3 - Engineering Improvements

- [ ] Global Exception Middleware
- [ ] Result Pattern
- [ ] Unit of Work
- [ ] Logging
- [ ] Health Checks
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