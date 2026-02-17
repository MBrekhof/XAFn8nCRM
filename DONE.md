# DONE

Completed tasks for the n8nCRM proof-of-concept. Moved here from `TODO.md`.

---

## 2026-02-17 — Project Kickoff

### Phase 1: Scope & Setup
- [x] **Define project scope and success criteria** — Established scope: XAF CRM with Customer/Order/Invoice, n8n integration via webhook + OData, Docker deployment, SQLite database. User confirmed: webhook push for invoice trigger, n8n email for daily report, simple Invoice entity.
- [x] **Create CLAUDE.md** — Project conventions, build commands, coding standards, XAF patterns, API auth flow, n8n integration points.
- [x] **Create TODO.md and DONE.md** — Task tracking documentation.
- [x] **Create README.md** — Project overview and setup instructions.
- [x] **Initialize Git repo and push to GitHub** — Private repo `n8ncrm` under MBrekhof.

### Phase 2: Assess & Compose
- [x] **Assessed work** — ~16 files, tight coupling for C# (domain→registration→seed), Docker/n8n independent. All Tier 0-1 risk.
- [x] **Selected subagents mode** — Parallel independent tasks via Task tool.

### Phase 3: Decompose
- [x] **Created 7 tasks** with dependencies and risk tiers. No file overlap between parallel tasks.

### Phase 4: Execute

#### Domain Model
- [x] **Customer.cs** — Name (required), Email, Phone, Address, Notes, Orders collection.
- [x] **Order.cs** — OrderNumber, OrderDate, Status enum, Customer ref, OrderItems (aggregated), TotalAmount (computed), Invoice ref.
- [x] **OrderItem.cs** — ProductName, Quantity, UnitPrice, LineTotal (computed), Order ref.
- [x] **Invoice.cs** — InvoiceNumber, InvoiceDate, Status enum, TotalAmount, Notes, Order ref.
- [x] **OrderStatus.cs** — Enum: Draft, Confirmed, Fulfilled, Cancelled.
- [x] **InvoiceStatus.cs** — Enum: Draft, Sent, Paid.

#### Entity Registration
- [x] **n8nCRMDbContext.cs** — Added DbSet<Customer>, DbSet<Order>, DbSet<OrderItem>, DbSet<Invoice>.
- [x] **Module.cs** — Added AdditionalExportedTypes for all 4 entities.
- [x] **Startup.cs** — Added BusinessObject<T> OData registrations for all 4 entities.

#### Seed Data
- [x] **Updater.cs** — SeedCrmData() with 3 customers (Acme, Globex, Initech), 3 orders with items, CRM CRUD permissions for Default role.

#### Webhook Integration
- [x] **OrderFulfilledController.cs** — XAF ViewController<DetailView> targeting Order. Detects Status→Fulfilled, fires HTTP POST webhook with OrderId/OrderNumber/CustomerName/TotalAmount/FulfilledAt payload.
- [x] **appsettings.json** — Added Webhooks:OrderFulfilled config pointing to `http://n8n:5678/webhook/order-fulfilled`.

#### Docker Setup
- [x] **Dockerfile** — Multi-stage build (sdk:8.0 → aspnet:8.0), DEVEXPRESS_NUGET_URL build arg, layer caching for restore, libicu for globalization.
- [x] **docker-compose.yml** — xafapp (port 5000:8080, sqlite volume) + n8n (port 5678, n8n-data volume), shared crm-network bridge.
- [x] **.dockerignore** — Excludes bin/obj, .vs, .git, *.db, node_modules.

#### n8n Workflows
- [x] **order-fulfilled-create-invoice.json** — 5-node workflow: Webhook → Authenticate → Prepare Invoice → Create Invoice (OData POST) → Respond.
- [x] **daily-orders-report.json** — 6-node workflow: Schedule (8am) → Authenticate → Calculate Date → Get Orders (OData GET with $filter/$expand) → Format HTML Report → Send Email (SMTP).
- [x] **n8n/workflows/README.md** — Import instructions, configuration checklist, testing guide.

#### Build Verification
- [x] **dotnet build** — 0 errors, 0 warnings. Build succeeded.
- [x] **JSON validation** — Both n8n workflow files validated as valid JSON with correct structure.
