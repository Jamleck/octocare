# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Octocare — an experimental NDIS (National Disability Insurance Scheme) management application.

## Spec

See [SPEC.md](SPEC.md) for the full product specification including architecture, domain model, and phasing.

## Stack

- **Backend:** ASP.NET Core (C#)
- **Frontend:** React (Vite SPA), shadcn/ui + Tailwind CSS
- **Database:** PostgreSQL (event-sourced financial ledger, RLS for multi-tenancy)
- **Auth:** External IdP (Auth0 or Azure AD B2C) + custom RBAC
- **Hosting:** Azure (Australia East)
- **Repo structure:** Monorepo — `src/api/` (ASP.NET Core), `src/web/` (Vite + React)

## Status

Features 1–4 implemented. .NET Aspire AppHost added for local orchestration. Backend and frontend build successfully with all tests passing (46 backend, 16 frontend).

### Completed
- **Database & Multi-Tenancy:** Domain entities (Organisation, User, UserOrgMembership, Participant), EF Core configurations with snake_case PostgreSQL columns, global query filters for tenant isolation, RLS migration, repository interfaces/implementations, dev data seeder
- **Authentication & Authorization:** Auth0 integration (JWT bearer), custom RBAC with role-permission mapping (org_admin, plan_manager, finance), ASP.NET Core policy-based authorization, CurrentUserService with per-request caching
- **Organisation & User Management:** API endpoints (GET/PUT /api/organisations/current, GET/POST/PUT /api/organisations/current/members), OrganisationService, MemberService, ABN validation (weighted checksum), frontend org settings page, team members page with invite dialog
- **Participant CRUD:** API endpoints (GET/POST/PUT /api/participants, POST deactivate), NDIS number validation, paginated search, frontend list/create/detail/edit pages with shared form component
- **Frontend:** React Router, Auth0 React SDK, AppLayout with nav, shadcn/ui components, API client with token integration, client-side validation utilities
- **Aspire:** AppHost (13.1.1) orchestrates PostgreSQL (persistent volume), API, and Vite frontend. ServiceDefaults provides OpenTelemetry, health checks, service discovery, and HTTP resilience.

## Running (Aspire)

```bash
dotnet run --project src/api/Octocare.AppHost
```

This starts PostgreSQL (Docker), the API, and the Vite dev server. The Aspire dashboard is available at the URL printed on startup.

## Build & Test

```bash
# Backend
cd src/api && dotnet build Octocare.slnx
cd src/api && dotnet test Octocare.slnx

# Frontend
cd src/web && pnpm install
cd src/web && pnpm run build
cd src/web && pnpm run test
```
