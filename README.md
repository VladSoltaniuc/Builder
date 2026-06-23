# Product Catalog — CRUD Best Practice Demo

Full-stack CRUD app: React + .NET 8 Web API.

---

## How to Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Node.js 22+](https://nodejs.org/en/download)
- [PostgreSQL](https://www.enterprisedb.com/downloads/postgres-postgresql-downloads) (installed locally)
- [HeidiSQL](https://www.heidisql.com/download.php) (tool to inspect the database)

### Backend
```bash
cd backend
dotnet ef database update   # creates DB and applies migrations
dotnet run                  # starts API on http://localhost:5080
```

### Frontend
```bash
cd frontend
npm install
npm run dev                 # starts UI on http://localhost:5173
```

> **Note:** Copy your connection string into `backend/appsettings.Development.json` before running. This file is gitignored — you must create it manually.

---

## Backend Architecture

### ✓ Important

- **Contracts(DTOs)** — data contracts. Define exactly what shape data must have going in and out of the API. *(Application layer)*
- **Controllers** — the entry point between frontend and backend. Minimal code — routes requests to services and returns the response. *(Presentation layer)*
- **Data** — database context (`AppDbContext`). Connects EF Core to the database and exposes tables as queryable collections. *(Infrastructure layer)*
- **Migrations/** — EF Core migration files. Auto-generated when you run `dotnet ef migrations add`. Each file is a snapshot of a schema change — never edit manually. *(Infrastructure layer)*
- **Models** — business entities. The raw data model used internally, never exposed directly. *(Domain layer)*
- **Services** — business logic. Everything the app can do lives here. *(Application layer)*
- **appsettings.json** — app configuration. Holds environment-specific values like connection strings and feature flags. Never hardcode these in code, place them here.
- **appsettings.Development.json** — local overrides for development. Gitignored — contains secrets like passwords. Automatically merged on top of `appsettings.json` when running locally.
- **Program.cs** — startup wiring. Registers services, middleware, and CORS so the app knows how to run. *(Composition root)*


### ⚠ Ignore — auto-generated or scaffolding

- **bin/** — compiled output produced by `dotnet build`. Never edit.
- **obj/** — intermediate build files produced by `dotnet build`. Never edit.
- **Properties/** — dev launch profiles (port, environment). Only relevant if you want to run local debugging (breakpoints, etc)
- **ProductApi.csproj** — project definition file. Managed by the framework — only touch it to add/remove NuGet packages.


## Database Architecture

- **Engine**: PostgreSQL 18 — runs locally, installed on the developer's machine.
- **ORM**: Entity Framework Core 8 with Npgsql driver — translates C# LINQ queries into SQL, no raw SQL needed for basic operations.
- **DbContext** (`Data/AppDbContext.cs`) — the single entry point to the database. Exposes tables as `DbSet<T>` collections you query like C# lists.
- **Migrations** (`Migrations/`) — version control for your database schema. Every schema change is a new migration file. Run `dotnet ef migrations add <Name>` to create one, `dotnet ef database update` to apply it.
- **Connection string** — stored in `appsettings.Development.json` (gitignored). Never committed. In production, injected via environment variables.

### Tables

| Table | Description |
|---|---|
| `Products` | Main product catalog — name, category, price, stock, version |
| `__EFMigrationsHistory` | EF Core internal — tracks which migrations have been applied. Never touch it. |


## Frontend Architecture (WIP)


## Shared

- **.gitignore** — tells Git which files not to track. Excludes auto-generated folders (`bin/`, `obj/`, `node_modules/`, `dist/`) — these can be hundreds of megabytes and are always rebuilt locally, so committing them would bloat the repository for no reason. If you ever randomly see 10.000 file changes in your git commit, this is likely the culprit.
- **README.md** — this file. Project documentation for anyone new to the codebase.
- **.claude/** — Claude Code configuration. Contains project-specific instructions for the AI assistant.

---

## Folder Structure

```
Builder/
├── backend/
│   ├── bin/                        # Auto-generated — ⚠ ignore
│   ├── Contracts/                  # Application layer — ✓ important
│   ├── Controllers/                # Presentation layer — ✓ important
│   ├── Data/                       # Infrastructure layer — ✓ important
│   ├── Migrations/                 # EF Core schema history — ✓ important
│   ├── Models/                     # Domain layer — ✓ important
│   ├── obj/                        # Auto-generated — ⚠ ignore
│   ├── Properties/                 # Dev launch profiles — ⚠ ignore
│   ├── Services/                   # Application layer — ✓ important
│   ├── appsettings.Development.json# Local secrets — ✓ important
│   ├── appsettings.json            # App configuration — ✓ important
│   ├── ProductApi.csproj           # Project definition — ⚠ ignore
│   └── Program.cs                  # Composition root — ✓ important
│
└── frontend/
    ├── src/
    │   ├── api/            # Transport + API layers — HTTP calls, error handling
    │   ├── hooks/          # Service layer — data fetching, pagination, mutations
    │   ├── components/     # Presentation layer — form and table
    │   ├── types/          # TypeScript types
    │   ├── constants/      # Shared labels and validation messages
    │   └── App.tsx         # UI orchestration layer
    ├── index.html          # Shell HTML — Vite injects the built JS here
    ├── package.json        # npm dependencies and scripts
    ├── vite.config.ts      # Vite bundler config
    └── tsconfig.json       # TypeScript compiler config
```
