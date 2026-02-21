# Octocare — NDIS Management Platform Specification

## 1. Product Overview

Octocare is a SaaS multi-tenant platform for NDIS (National Disability Insurance Scheme) plan management. It enables plan management organisations to manage participant budgets, process provider invoices, submit claims to the NDIA, and track spending in real time.

**Target users (by persona):**

| Persona | Role | MVP? |
|---|---|---|
| Plan Manager | Core user. Manages participants, budgets, invoices, claims | Yes |
| Finance Staff | Processes payments, reconciles claims, handles reporting | Yes |
| Org Admin | Manages users, org settings, onboarding | Yes |
| Support Coordinator | Manages referrals, service connections, monitors delivery | Post-MVP |
| Service Provider | Submits invoices, manages service bookings | Post-MVP (portal) |
| Participant | Views budget and service history | Out of scope |

**Team:** Solo developer (with AI assistance) initially, with plans to hire as product is validated.

---

## 2. Technical Architecture

### 2.1 Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core (C#) |
| Frontend | React / Next.js |
| UI Components | shadcn/ui + Tailwind CSS |
| Database | PostgreSQL |
| Event Store | PostgreSQL (JSONB event payloads) |
| Auth Provider | External IdP (Auth0 or Azure AD B2C) |
| Hosting | Azure (Australia East region) |
| Repository | Monorepo (API + frontend in one repo) |

### 2.2 Monorepo Structure

```
octocare/
├── src/
│   ├── api/                    # ASP.NET Core Web API
│   │   ├── Octocare.Api/       # HTTP layer, controllers, middleware
│   │   ├── Octocare.Domain/    # Domain models, events, aggregates
│   │   ├── Octocare.Application/ # Use cases, command/query handlers
│   │   ├── Octocare.Infrastructure/ # EF Core, event store, external integrations
│   │   └── Octocare.Tests/     # Unit + integration tests
│   └── web/                    # Next.js frontend
│       ├── app/                # Next.js app router
│       ├── components/         # shadcn/ui + custom components
│       ├── lib/                # API client, utilities
│       └── __tests__/          # Frontend tests
├── shared/                     # Shared types/contracts (OpenAPI generated)
├── tools/                      # Scripts, price guide importers, migrations
├── CLAUDE.md
├── SPEC.md
└── README.md
```

### 2.3 Event-Sourced Financial Ledger

The financial subsystem uses event sourcing. Every mutation to a participant's financial state is an immutable event. This provides a complete audit trail, point-in-time reconstruction, and compliance with NDIS audit requirements.

**Event store design (PostgreSQL):**

- Events table: `id`, `stream_id`, `stream_type`, `event_type`, `payload` (JSONB), `metadata` (JSONB), `version`, `created_at`
- Streams represent aggregates (e.g., a participant's plan budget, an invoice)
- Optimistic concurrency via version column

**Example event types:**

```
PlanCreated, BudgetAllocated, InvoiceSubmitted, InvoiceApproved,
InvoiceRejected, ClaimSubmitted, ClaimAccepted, ClaimRejected,
PaymentReceived, ServiceBookingCreated, ServiceBookingCancelled
```

**Read model strategy — Hybrid CQRS (recommended):**

Since the team is solo and the system needs to be operationally simple:

- **In-line projections** for hot operational data: current budget balances, invoice statuses, active service bookings. Updated synchronously in the same transaction as event writes. Stored in standard PostgreSQL tables queried by the API.
- **Async projections** for reporting and analytics: spending trends, burn rate calculations, utilisation reports. Updated via a background worker that processes the event stream. Can be rebuilt from scratch if needed.
- **Single PostgreSQL database** for both events and projections. No separate read database until scale demands it.

This avoids the operational overhead of a dedicated event store or separate read database while preserving the core benefits of event sourcing (auditability, replayability).

### 2.4 Multi-Tenancy

**Recommendation: Row-level security (RLS) with PostgreSQL.**

Given solo developer, PostgreSQL choice, and the need to keep ops simple:

- Every tenant-scoped table has a `tenant_id` column
- PostgreSQL RLS policies enforce isolation at the database level
- Application sets `app.current_tenant` session variable on every connection
- RLS acts as a safety net — application code also filters by tenant, but RLS prevents bugs from leaking data
- Provider records exist in a shared space (providers work across tenants) with junction tables for tenant-provider relationships

**Why not database-per-tenant:** Operationally expensive for a solo dev. Migrations, backups, and connection pooling become multiplicative. RLS with disciplined application code is sufficient for this stage. Can migrate to database-per-tenant later if a customer requires it for compliance.

### 2.5 Authentication & Authorization

**Authentication:** External IdP (Auth0 or Azure AD B2C)

- Handles registration, login, MFA, password reset, social login
- Issues JWTs validated by the ASP.NET Core API
- Manages email verification and account lifecycle

**Authorization:** Custom RBAC in application database

- **Global identity:** One account per person (email-based), can belong to multiple orgs
- **Org membership:** `user_org_memberships` table with `user_id`, `org_id`, `role`
- **Roles:** `org_admin`, `plan_manager`, `finance`, `support_coordinator`, `provider`
- **Permissions:** Role-to-permission mapping, checked via middleware/policy handlers
- **Provider cross-org access:** Providers link their identity to multiple orgs. Each org-provider relationship has its own status and permissions

---

## 3. Domain Model

### 3.1 Core Entities

**Organisation** — A plan management company (tenant)
- ABN, name, contact details, PRODA credentials (encrypted)
- Subscription tier, billing details

**Participant** — An NDIS participant managed by the organisation
- NDIS number, name, date of birth, contact details
- Nominee/guardian information
- Active plan reference

**Plan** — An NDIS plan for a participant
- Plan number, start date, end date
- Management type: plan-managed only (agency-managed and self-managed categories are not tracked)
- Status: `draft`, `active`, `expiring`, `expired`, `transitioned`

**Budget Category** — A budget allocation within a plan
- Support category (Core, Capacity Building, Capital)
- Support purpose (e.g., Daily Activities, Social & Community Participation)
- Allocated amount, committed amount, spent amount (projected from events)
- Linked to a price guide version

**Service Agreement** — Contract between participant and provider
- Provider reference, participant reference
- Agreed support items, rates, frequency
- Start date, end date, status: `draft`, `sent`, `active`, `expired`, `terminated`
- Signed document attachment

**Service Booking** — A committed allocation of budget to a provider
- Links service agreement to budget category
- Allocated amount, used amount
- Status: `active`, `completed`, `cancelled`

**Invoice** — A provider's request for payment
- Provider, participant, service period
- Line items (each referencing a price guide item, quantity, rate, amount)
- Status: `submitted`, `under_review`, `approved`, `rejected`, `disputed`, `paid`
- Source: `provider_portal`, `email_ingestion`, `manual_entry`
- Validation results (rate check, plan period check, budget check)

**Claim** — A payment request submitted to the NDIA
- Batch of approved invoice line items
- Status: `draft`, `submitted`, `accepted`, `partially_rejected`, `rejected`
- NDIA reference number, submission date, response date
- Rejection reasons per line item

**Provider** — A service provider (shared across tenants)
- ABN, name, contact details
- Registration groups, service types
- GST registration status
- Per-tenant relationship status

### 3.2 Plan Transfer (Recommended Approach)

When a participant changes plan manager (Org A → Org B):

- **Clean handoff with summary export.** Org A retains all historical records. Org B starts fresh.
- Org A generates a **transition summary**: remaining budget per category, outstanding invoices, active service bookings, pending claims.
- This summary is exportable as a structured document (PDF + JSON) that Org B can import to bootstrap the participant's record.
- Active service bookings are flagged for provider re-confirmation with the new PM.
- In-flight invoices remain with Org A until resolved; new invoices go to Org B.
- No cross-tenant data sharing — each org's data stays in their tenant.

---

## 4. NDIS Price Guide Engine

### 4.1 Versioned Price Guide

- Multiple price guide versions coexist in the system (e.g., 2024-25, 2025-26)
- Each version has an effective date range
- Invoices validate against the price guide version active on the **service delivery date**, not the invoice submission date
- Service bookings that span a price guide boundary (e.g., June–August) automatically reference both versions

### 4.2 Price Guide Data Model

```
PriceGuideVersion
  - id, name (e.g., "2025-26"), effective_from, effective_to

SupportItem
  - id, version_id, item_number (e.g., "01_011_0107_1_1")
  - name, description, registration_group
  - support_category, support_purpose
  - unit (hour, each, day, week)
  - price_limit (national), price_limit_remote, price_limit_very_remote
  - is_ttp_eligible (Temporary Transformation Payment)
  - cancellation_rule (short_notice_7_day, short_notice_2_day, none)
  - is_non_face_to_face_eligible
  - is_provider_travel_eligible
  - claim_type (time, non_time)
```

### 4.3 Rate Validation

When an invoice is submitted, the system validates each line item:

1. **Support item exists** in the applicable price guide version
2. **Rate does not exceed** the price limit (including applicable loadings)
3. **Loadings are valid:** TTP, remote/very remote, public holiday, weekend
4. **Service date** falls within the participant's plan period
5. **Budget category** has sufficient remaining funds
6. **Provider is registered** for the relevant registration group (if data available)

Validation produces warnings (soft) and errors (hard). Hard errors block approval. Warnings flag for PM review but allow approval.

### 4.4 Cancellation Rules (Guided Process)

When a cancellation-related invoice is submitted:

- System identifies the applicable cancellation rule for the support item
- Displays the rule to the plan manager (e.g., "Short notice cancellation requires < 2 business days notice for this support type")
- PM confirms whether the cancellation fee is valid based on the circumstances
- Decision is recorded in the event stream with the PM's rationale

---

## 5. Invoice Processing Pipeline

### 5.1 Three Ingestion Paths

**Path 1 — Provider Portal (post-MVP):**
Provider logs in → selects participant → selects service booking → enters line items → system pre-validates → submits to PM queue.

**Path 2 — Email Ingestion with AI Extraction (post-MVP):**
Provider emails invoice PDF → system receives via dedicated email address → AI (LLM-based document extraction) parses PDF → extracts: provider ABN, participant name/NDIS number, service dates, line items, rates, totals → presents extracted data to PM for confirmation → PM corrects any errors → submits to processing pipeline.

AI extraction details:
- Use Azure Document Intelligence or an LLM with structured output for field extraction
- Always requires human confirmation — no auto-processing
- Extraction confidence displayed per field so PM knows what to double-check
- Unrecognized formats fall back to manual entry with the PDF displayed side-by-side

**Path 3 — Manual Entry (MVP):**
PM manually enters invoice details with the PDF displayed alongside the form. Form auto-suggests support items and validates rates in real-time.

### 5.2 Invoice Lifecycle

```
submitted → under_review → approved → [claim_submitted] → paid
                         → rejected (with reason)
                         → disputed → under_review → approved/rejected
```

### 5.3 Dispute Resolution Workflow

When a PM flags an issue with an invoice:

1. PM changes status to `disputed` with a structured reason (rate_incorrect, service_not_delivered, outside_plan_period, item_not_in_plan, other)
2. System notifies provider (email) with dispute details
3. Provider responds (via portal post-MVP, or PM records response manually for MVP)
4. Status moves to `under_review` with dispute context attached
5. PM resolves: approve (possibly with adjustments), reject, or escalate
6. Full dispute thread is preserved in the event stream

---

## 6. Claim Submission (Phased NDIA Integration)

### Phase 1 — CSV Export (MVP)

- PM selects approved invoices to include in a claim batch
- System generates NDIA-formatted bulk payment request CSV
- PM downloads and manually uploads to myplace portal
- PM manually records claim outcome (accepted/rejected per line) back in the system
- System reconciles: marks invoices as paid or flags rejections

### Phase 2 — B2G API Integration

- Use NDIA's official machine-to-machine APIs where available
- Automate claim submission and status polling
- Auto-reconcile accepted claims, flag rejections for PM review

### Phase 3 — Browser Automation (if needed)

- For workflows not covered by B2G APIs
- Headless browser automation (Playwright) as last resort
- Must be resilient to portal UI changes — circuit breaker pattern, alerts on failure

---

## 7. Budget Tracking & Projections

### 7.1 Real-Time Budget State

Budget state is derived from the event stream:

- **Allocated** = total plan allocation for the category
- **Committed** = sum of active service bookings
- **Spent** = sum of approved/paid invoices
- **Pending** = sum of submitted but not-yet-approved invoices
- **Available** = Allocated - Committed (or Allocated - Spent, depending on view)

### 7.2 Burn Rate & Projections

- Calculate weekly/monthly burn rate from spending history
- Project budget exhaustion date based on current pace
- Compare actual spending to plan period progress (e.g., "60% through plan period, 45% of budget spent — underspending")
- Detect sudden spending changes (e.g., new provider, increased service frequency)

### 7.3 Alerts

Configurable alerts (email-first, with in-app notification centre):

- Budget threshold crossed (e.g., 75%, 90% spent)
- Projected underspend at plan end (below configurable threshold)
- Projected overspend before plan end
- Plan expiry approaching (90 days, 60 days, 30 days)
- No invoices received for a participant in X weeks (may indicate service gap)

---

## 8. Plan Transitions

When a plan is approaching expiry, the system triggers a guided transition workflow:

1. **Alert phase (90 days before expiry):** Notify PM that plan review is upcoming
2. **Preparation checklist:**
   - Outstanding invoices on current plan (must be resolved before transition)
   - Active service bookings and their remaining allocations
   - Budget utilisation summary (to inform plan review evidence)
   - Pending claims not yet reconciled
3. **New plan creation:** PM creates new plan when NDIA confirms reassessment outcome
   - System prompts to recreate service bookings at new rates (if price guide has changed)
   - Budget comparison: old plan vs new plan allocations
4. **Transition execution:**
   - Old plan status → `transitioned`
   - New plan status → `active`
   - Service bookings migrated (with PM confirmation) or closed out
   - Any unresolved invoices flagged for attention

---

## 9. Service Agreements

### 9.1 Lifecycle

```
draft → sent → active → expiring → expired
                      → terminated (early exit)
```

### 9.2 Features

- **Template library:** Pre-built templates for common NDIS support types. Customisable per org.
- **Agreement creation:** Select participant, provider, support items, rates (validated against price guide), frequency, start/end dates.
- **E-signature:** Integration with a signing service (e.g., DocuSign or a simpler solution like embedded signature pads). Post-MVP — for MVP, upload signed PDF.
- **Delivery tracking:** Compare invoiced services against agreement terms. Alert if a provider is over- or under-delivering relative to the agreement.
- **Expiry management:** Auto-alert when agreements are approaching end date. Prompt for renewal or termination.

---

## 10. Reporting

### 10.1 Pre-Built Reports

| Report | Description |
|---|---|
| Budget Utilisation | Per-participant spending vs allocation by category, with projections |
| Provider Spending | Total paid per provider, per participant, per period |
| Claim Status | All claims by status, submission date, response time |
| Outstanding Invoices | Aging report of unpaid/unprocessed invoices |
| Service Agreement Status | Active, expiring, expired agreements across all participants |
| Audit Trail | Full event history for a participant, filterable by date and event type |
| Participant Summary | Single-page overview per participant: plan, budget, services, recent activity |

### 10.2 Export

- All reports exportable to CSV and Excel
- Raw event data exportable for external analysis
- Optional integration point for BI tools (expose read-only database replica or API)

---

## 11. Notifications

**Email-first approach:**

- Critical events trigger immediate email: claim rejection, invoice dispute, plan expiry (30 days)
- Routine events bundled into configurable daily digest: new invoices received, budget threshold warnings, service agreement expiry warnings
- In-app notification centre as secondary channel (badge count, mark as read)
- User preferences: per-notification-type control over immediate vs digest vs off
- Org-level defaults that individual users can override

---

## 12. Money Handling

**Recommendation: Integer cents (long) for all monetary values.**

- All amounts stored as integer cents (e.g., $1,234.56 → 123456)
- Eliminates floating point precision issues entirely
- Convert to dollars only at the API response / display layer
- For rate calculations with loadings (e.g., $65.47/hr × 1.175 TTP loading):
  - Calculate in cents: 6547 × 1175 / 1000 = 7692.725 → round to 7693 (banker's rounding)
  - Rounding applied per line item, not to totals
- Claim totals are sum-of-rounded-line-items (matching NDIA behaviour)
- All rounding logic centralised in a single `Money` value object / utility

---

## 13. Privacy & Security

### 13.1 Pragmatic Baseline (MVP)

- **Encryption in transit:** TLS 1.2+ everywhere
- **Encryption at rest:** Azure-managed encryption for database and blob storage
- **Data residency:** All data in Azure Australia East
- **Authentication:** MFA available via IdP, enforced for org admins
- **Authorization:** Custom RBAC with least-privilege defaults
- **Row-level security:** PostgreSQL RLS as a defence-in-depth layer for tenant isolation
- **Secrets management:** Azure Key Vault for PRODA credentials, API keys
- **Logging:** Structured audit logs for all data access and mutations (inherent in event sourcing)
- **HTTPS only:** HSTS headers, secure cookies

### 13.2 Compliance Roadmap (Post-MVP)

- Formal Privacy Impact Assessment
- Data breach notification procedure (per Notifiable Data Breaches scheme)
- Consent management for participant data
- Data retention and deletion policies
- Penetration testing before production launch
- SOC 2 Type I (if targeting enterprise customers)

---

## 14. MVP Scope & Phasing

### MVP (Phase 1) — Plan Manager Core

**Goal:** Validate the core plan management workflow for a single org.

**In scope:**
- Org setup and user management (admin, plan manager, finance roles)
- Participant CRUD with NDIS number, basic details
- Plan creation with budget categories and allocations
- Manual invoice entry with price guide validation
- Invoice approval/rejection workflow
- Invoice dispute workflow (PM-side only, provider notified via email)
- Claim batch creation and CSV export (NDIA format)
- Manual claim reconciliation (PM records outcome)
- Real-time budget tracking with projections and burn rate
- Budget threshold alerts (email)
- Plan expiry alerts and guided transition workflow
- Pre-built reports (budget utilisation, outstanding invoices, claim status, audit trail)
- CSV/Excel export for all reports
- Price guide import tool (support for multiple versions)
- Service agreement creation from templates, upload signed PDF, expiry tracking

**Not in scope for MVP:**
- Provider portal (providers interact via email/phone, PM enters invoices)
- Email/OCR invoice ingestion
- Participant-facing access
- NDIA portal automation (B2G API or browser)
- E-signature integration
- Support coordinator workflows
- GST handling

### Phase 2 — Provider Portal & AI Ingestion

- Provider self-service portal (invoice submission, payment status tracking)
- Email ingestion with AI extraction + human review
- Cancellation fee guided validation
- Service agreement delivery tracking
- Enhanced reporting (provider spending, service agreement status)

### Phase 3 — NDIA Integration & Scale

- B2G API integration for claim submission and reconciliation
- Browser automation for remaining portal workflows
- Support coordinator persona and workflows
- Advanced budget projections (ML-based spending pattern analysis)
- BI tool integration (read replica or API)

### Phase 4 — Enterprise & Compliance

- Database-per-tenant option for enterprise customers
- SOC 2 compliance
- Participant portal (read-only budget/service view)
- White-label/custom branding per org
- Full GST engine if market demands it

---

## 15. Key Technical Risks

| Risk | Impact | Mitigation |
|---|---|---|
| NDIA changes portal/CSV format without notice | Claims fail to process | Version the CSV generator, monitor for format changes, alert on submission failures |
| Price guide restructuring (not just rate changes) | Support items renumbered, categories changed | Price guide versioning handles this, but migration tooling needed for mapping old → new items |
| Event store grows large over time | Slow projections, storage costs | Snapshot strategy for aggregates, archive old events to cold storage, partition events table by date |
| Solo developer bottleneck | Slow iteration, single point of failure | Keep architecture simple, lean on managed services, automate everything, document thoroughly |
| RLS misconfiguration leaks tenant data | Data breach, regulatory consequences | RLS as defence-in-depth (not sole mechanism), integration tests that verify isolation, regular security audits |
| AI invoice extraction accuracy | PM spends more time correcting than entering manually | Always human-in-the-loop, track extraction accuracy metrics, fall back to manual if accuracy drops below threshold |

---

## 16. Open Questions

These need answers before or during development:

1. **IdP choice:** Auth0 vs Azure AD B2C. Auth0 has better DX, Azure AD B2C is cheaper at scale and integrates with Azure ecosystem. Recommend Auth0 for MVP speed, evaluate migration later.
2. **NDIA B2G API access:** Need to apply for API access through PRODA. Lead time and approval process unknown. Start CSV export and investigate in parallel.
3. **Price guide data source:** NDIA publishes the price guide as PDF and Excel. Need a reliable import pipeline. Consider scraping the NDIS website or using community-maintained machine-readable versions.
4. **E-signature provider:** DocuSign is expensive. Alternatives: SignNow, HelloSign, or build a simple in-app signature capture for MVP.
5. **Email ingestion infrastructure:** Need a dedicated email receiving service (e.g., SendGrid Inbound Parse, AWS SES) for the OCR pipeline. Architecture decision needed before Phase 2.
