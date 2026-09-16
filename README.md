# n8nCRM

A proof-of-concept integrating [DevExpress XAF](https://www.devexpress.com/products/net/application_framework/) Blazor Server with [n8n](https://n8n.io/) workflow automation, demonstrating how a modern .NET CRM application can leverage low-code workflow automation for business process orchestration.

## What This Demonstrates

- **DevExpress XAF Blazor Server** as a full-featured CRM with auto-generated UI, security, and OData API
- **n8n workflow automation** for business process orchestration
- **OData/REST API** as the integration layer between the two systems
- **Docker Compose** deployment for both services on a single Linux VPS

## Architecture

```
┌─────────────────────┐         ┌─────────────────────┐
│   XAF Blazor CRM    │         │        n8n           │
│                     │         │                     │
│  ┌───────────────┐  │ webhook │  ┌───────────────┐  │
│  │  Order Status  │──┼────────┼──│  Webhook Node  │  │
│  │  → Fulfilled   │  │         │  │               │  │
│  └───────────────┘  │         │  └──────┬────────┘  │
│                     │         │         │           │
│  ┌───────────────┐  │  OData  │  ┌──────▼────────┐  │
│  │  OData API    │◄─┼────────┼──│ Create Invoice │  │
│  │  /api/odata   │  │         │  │  via API       │  │
│  └───────────────┘  │         │  └───────────────┘  │
│                     │         │                     │
│  ┌───────────────┐  │  OData  │  ┌───────────────┐  │
│  │  Orders Data  │◄─┼────────┼──│ Daily Report   │  │
│  │               │  │  query  │  │ (Scheduled)    │  │
│  └───────────────┘  │         │  └──────┬────────┘  │
│                     │         │         │           │
│                     │         │  ┌──────▼────────┐  │
│                     │         │  │  Send Email    │  │
│                     │         │  └───────────────┘  │
└─────────────────────┘         └─────────────────────┘
        SQLite DB                    Docker Volume
```

## Domain Model

| Entity | Description |
|--------|-------------|
| **Customer** | Company/contact with name, email, phone |
| **Order** | Belongs to a Customer, has a status (Draft → Confirmed → Fulfilled → Cancelled) |
| **OrderItem** | Line item: product, quantity, unit price |
| **Invoice** | Auto-created by n8n when an Order is fulfilled |

## Tech Stack

| Component | Technology |
|-----------|-----------|
| CRM Application | DevExpress XAF Blazor Server 26.1 (.NET 10.0) |
| Database | SQLite (EF Core 10) |
| API | OData v4.01 with JWT authentication |
| Workflow Engine | n8n (community edition) |
| Containerization | Docker + Docker Compose |
| Target Deployment | Linux VPS |

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://docs.docker.com/get-docker/) and [Docker Compose](https://docs.docker.com/compose/install/)
- [DevExpress NuGet feed](https://docs.devexpress.com/GeneralInformation/116042/installation/install-devexpress-controls-using-nuget-packages) configured with a valid license

## Quick Start

### Local Development

```bash
# First run only: create/update the SQLite database (the app only does this
# automatically when a debugger is attached)
dotnet run --project n8nCRM/n8nCRM.Blazor.Server/n8nCRM.Blazor.Server.csproj -- -updateDatabase -silent

# Build and run the XAF app
dotnet run --project n8nCRM/n8nCRM.Blazor.Server/n8nCRM.Blazor.Server.csproj

# Access the app at https://localhost:5001
# Login: Admin / (empty password)
```

### Docker Compose (Full Stack)

```bash
# Start both XAF app and n8n
docker-compose up --build

# XAF CRM:  http://localhost:5000
# n8n:      http://localhost:5678
```

## API Usage

```bash
# 1. Authenticate
TOKEN=$(curl -s -X POST http://localhost:5000/api/Authentication/Authenticate \
  -H "Content-Type: application/json" \
  -d '{"userName":"Admin","password":""}')

# 2. Query customers
curl http://localhost:5000/api/odata/Customer \
  -H "Authorization: Bearer $TOKEN"

# 3. Query orders
curl http://localhost:5000/api/odata/Order \
  -H "Authorization: Bearer $TOKEN"
```

## n8n Workflows

| Workflow | Trigger | Action |
|----------|---------|--------|
| **Invoice Creation** | Webhook (Order fulfilled) | Creates an Invoice via XAF OData API |
| **Daily Orders Report** | Scheduled (daily) | Queries new orders, emails summary |

## Project Documentation

- [`CLAUDE.md`](CLAUDE.md) — Development conventions and project guide
- [`TODO.md`](TODO.md) — Active task list
- [`DONE.md`](DONE.md) — Completed tasks log

## License

This project uses DevExpress components which require a valid [DevExpress license](https://www.devexpress.com/buy/).
