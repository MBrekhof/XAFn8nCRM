# TODO

Tracked tasks for the n8nCRM proof-of-concept. Items move to `DONE.md` when completed.

---

## Phase 1: Scope & Setup
- [x] Define project scope and success criteria
- [x] Create CLAUDE.md with conventions and project guide
- [x] Create TODO.md and DONE.md for tracking
- [x] Create README.md
- [x] Initialize Git repo and push to GitHub
- [ ] Complete Phase 2: Assess & Compose (execution mode selection)

## Phase 2: Assess & Compose
- [ ] Analyze file count, coupling, and risk
- [ ] Select execution mode (single-session / subagents / agent-team)
- [ ] Define team composition if needed

## Phase 3: Decompose
- [ ] Break work into discrete tasks with dependencies
- [ ] Assign risk tiers and file ownership
- [ ] Present task graph for approval

## Phase 4: Execute

### Domain Model (XAF Business Objects)
- [ ] Create Customer business object
- [ ] Create Order and OrderItem business objects with status enum
- [ ] Create Invoice business object
- [ ] Register all entities in DbContext and Module
- [ ] Expose all entities via OData Web API
- [ ] Seed sample data in Updater

### Webhook Integration
- [ ] Implement webhook notification on Order status change to Fulfilled
- [ ] Create webhook configuration (target URL in appsettings)

### Docker Setup
- [ ] Create Dockerfile for XAF Blazor Server app
- [ ] Create docker-compose.yml (XAF app + n8n)
- [ ] Configure Docker networking and volumes
- [ ] Test containers communicate

### n8n Workflows
- [ ] Create n8n webhook workflow: receive Order Fulfilled → create Invoice via OData
- [ ] Create n8n scheduled workflow: daily new orders report → email

### Solution Housekeeping
- [x] ~~Create .sln file~~ — `n8nCRM.slnx` already exists (XML format)
- [x] Create .gitignore

## Phase 5: Close
- [ ] Final build verification
- [ ] Closeout summary
- [ ] Final commit and push
