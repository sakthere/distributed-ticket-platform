# Architecture Overview

```text
                 HTTP

                  │

                  ▼

      TicketManagement.Api

                  │

                  ▼

    TicketManagement.Application

                  │

        ┌─────────┴─────────┐

        ▼                   ▼

Infrastructure      Persistence

        ▼                   ▼

   JWT / BCrypt      EF Core / SQL

                  │

                  ▼

            SQL Server
```