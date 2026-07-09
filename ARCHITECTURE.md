# Architecture

## Goal

Build a production-grade Ticket Management System while learning modern .NET backend engineering.

This project is intentionally designed to demonstrate engineering principles rather than simply implementing CRUD functionality.

---

# Architectural Style

- Clean Architecture
- Vertical Slice Architecture
- CQRS (Commands and Queries)
- Dependency Injection
- Repository Pattern

---

# Solution Structure

TicketManagement.Api

Responsible for:

- HTTP Endpoints
- Authentication
- Authorization
- Dependency Injection
- Swagger

---

TicketManagement.Application

Responsible for:

- Business Use Cases
- Commands
- Queries
- Validators
- Interfaces

---

TicketManagement.Domain

Responsible for:

- Entities
- Enums
- Business Models

Contains no infrastructure code.

---

TicketManagement.Persistence

Responsible for:

- EF Core
- DbContext
- Entity Configurations
- Repositories
- SQL Server

---

TicketManagement.Infrastructure

Responsible for:

- JWT
- Password Hashing
- External Services
- Cross-cutting Infrastructure

---

# Request Flow

HTTP Request

↓

Controller

↓

Command Handler

↓

Repository

↓

EF Core

↓

SQL Server

↓

Response

---

# Current Features

- User Registration
- Login
- JWT Authentication
- Authorization

---

# Planned Features

- Ticket CRUD
- Comments
- Attachments
- Notifications
- Redis
- RabbitMQ
- Docker
- Serilog
- Microservices