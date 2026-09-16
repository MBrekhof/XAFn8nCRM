# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Proof-of-concept CRM integrating DevExpress XAF Blazor Server with n8n workflow automation via OData/REST API. Both services run in Docker containers on a Linux VPS.

## Tech Stack

- **Framework:** DevExpress XAF Blazor Server (.NET 10.0)
- **DevExpress Version:** 26.1.4 (explicit versions in .csproj — CPM is disabled)
  - Swashbuckle.AspNetCore must stay on 9.x: DevExpress.ExpressApp.WebApi compiles against Microsoft.OpenApi 1.x, Swashbuckle 10 fails at startup
  - Microsoft.CodeAnalysis.* is an exact pin from DevExpress.ExpressApp.EFCore (5.0.0 on net10.0)
- **Database:** SQLite via EF Core 10.0.12
- **API:** OData v4.01 + REST (JWT authenticated)
- **Workflow Automation:** n8n (community edition, Docker)
- **Containerization:** Docker + docker-compose

## Build & Run

```bash
# Restore and build (from repo root)
dotnet build n8nCRM.slnx

# Run locally
dotnet run --project n8nCRM/n8nCRM.Blazor.Server/n8nCRM.Blazor.Server.csproj

# Docker
docker-compose up --build
```

## Key Configuration

- **Connection string:** `appsettings.json` → `ConnectionStrings:ConnectionString`
  - Format: `EFCoreProvider=SQLite;Data Source=n8nCRM`
- **JWT signing key:** `appsettings.json` → `Authentication:Jwt:IssuerSigningKey`
- **OData route:** `/api/odata`
- **Swagger:** `/swagger` (Development environment only)
- **Auth endpoint:** `POST /api/Authentication/Authenticate`

## Coding Conventions

### C# / XAF

- **Business objects:** EF Core entities in `n8nCRM.Module/BusinessObjects/`
- **DbContext:** `n8nCRMEFCoreDbContext` — register all new DbSet<T> here
- **Module registration:** Add `AdditionalExportedTypes.Add(typeof(T))` in `Module.cs`
- **Web API exposure:** Register in `Startup.cs` → `webApiBuilder.ConfigureOptions` → `options.BusinessObject<T>()`
- **Immutable DTOs:** Use `record` types for any DTOs
- **Value objects:** Use `readonly record struct` for strongly-typed IDs and value types
- **Pattern matching:** Prefer `switch` expressions over if/else chains
- **Async:** Always pass `CancellationToken`, never block on `.Result` or `.Wait()`
- **Nullable:** Respect nullable reference types — no suppression without justification
- **No AutoMapper:** Use explicit mapping extension methods
- **Composition over inheritance:** Avoid abstract base classes in application code (XAF base classes like `BaseObject` are fine — framework requirement)
- **Sealed by default:** Mark classes `sealed` unless inheritance is needed

### XAF-Specific Patterns

- Business objects inherit from `DevExpress.Persistent.BaseImpl.EF.BaseObject` (provides `Guid ID`)
- Use `[DefaultProperty(nameof(PropertyName))]` for display in lookups
- Use `[DevExpress.ExpressApp.DC.Aggregated]` for owned collections
- Enums render as dropdowns automatically in XAF UI
- Collections use `virtual IList<T>` with `ObservableCollection<T>` initialization for EF Core change tracking
- Validation uses `DevExpress.Persistent.Validation` attributes (e.g., `[RuleRequiredField]`)

### Database

- SQLite — single file, volume-mounted in Docker for persistence
- EF Core handles schema updates via XAF's `DatabaseUpdateMode`
- `OnModelCreating` uses: deferred deletion, optimistic lock, `SetNull`/`Cascade` delete behavior

### Docker

- XAF app Dockerfile requires `DEVEXPRESS_NUGET_URL` build arg (stored in `.env`, git-ignored)
- Dockerfile restores individual .csproj files (kept from the SDK 8 days; `.slnx` restore works on SDK 10)
- n8n uses official `docker.n8n.io/n8nio/n8n` image
- Services communicate over Docker internal network (`crm-network`)
- SQLite DB file stored in a named volume (`sqlite-data` → `/app/data/`)

## Git Workflow

- Use feature branches: `feature/<name>`, `fix/<name>` — PR to master
- Solution file: `n8nCRM.slnx` (XML format, requires SDK 9+ for `dotnet restore` at solution level)
- First local run: `dotnet run --project n8nCRM/n8nCRM.Blazor.Server/n8nCRM.Blazor.Server.csproj -- -updateDatabase -silent` creates the SQLite DB; the app only updates the schema automatically with a debugger attached

## Default Users (Development)

| User  | Password | Role           |
|-------|----------|----------------|
| Admin | (empty)  | Administrators |
| User  | (empty)  | Default        |

## API Authentication Flow

1. `POST /api/Authentication/Authenticate` with `{ "userName": "Admin", "password": "" }`
2. Response: JWT token string
3. Use `Authorization: Bearer <token>` header for subsequent OData/API calls

## n8n Integration Points

- **Webhook endpoint (XAF → n8n):** XAF fires HTTP POST to n8n webhook when Order status changes to Fulfilled
- **OData API (n8n → XAF):** n8n queries/creates business objects via `/api/odata/Customer`, `/api/odata/Order`, `/api/odata/Invoice`
- **Daily report workflow:** n8n scheduled trigger queries orders from last 24h, generates and emails summary
