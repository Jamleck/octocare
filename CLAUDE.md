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

Monorepo scaffolded. Backend (ASP.NET Core clean architecture) and frontend (Vite + React) are buildable with passing tests.
