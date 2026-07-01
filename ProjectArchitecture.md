# Builder - Full-Stack CRUD Best-Practice Demo

Full-stack app: React + .NET 8 Web API. Products, users, and orders CRUD, plus
JWT/Google/2FA auth, a database audit trail, live order updates (SignalR),
scheduled email/SMS reports, and i18n.

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

> **Note:** Copy your connection string into `backend/appsettings.Development.json` before running. This file is gitignored - you must create it manually.

### Tests

Set the test database connection string once (bash):
```bash
echo 'export TEST_CONNECTION_STRING="Host=127.0.0.1;Port=5432;Database=builder_test;Username=<user>;Password=<password>"' >> ~/.bashrc
source ~/.bashrc
```

Then run:
```bash
# Unit tests
dotnet test tests/ProductApi.UnitTests

# Integration tests (requires PostgreSQL running)
dotnet test tests/ProductApi.IntegrationTests

# All tests
dotnet test
```

> Integration tests use a separate `builder_test` database. It is created and migrated automatically on first run.

---

## Backend Architecture

### ✓ Important

- **Contracts/** - data contracts (DTOs). Define exactly what shape data must have going in and out of the API: requests, responses, query objects. *(Application layer)*
- **Controllers/** - the entry point between frontend and backend. Minimal code - routes requests to services and returns the response. *(Presentation layer)*
- **Services/** - business logic. Everything the app can do lives here, behind an interface per service. *(Application layer)*
- **Models/** - business entities and the read model backing the audit-report view. The raw data model used internally, never exposed directly. *(Domain layer)*
- **Data/** - database context (`AppDbContext`), the EF interceptors, and the admin seeder. Connects EF Core to the database and exposes tables as queryable collections. *(Infrastructure layer)*
- **Configuration/** - strongly-typed settings classes bound from `appsettings` sections (Jwt, Google, App, AdminSeed, …). *(Infrastructure layer)*
- **Constants/** - shared compile-time constants and defaults (pagination, image/upload limits, …). *(Shared layer)*
- **Infrastructure/** - cross-cutting helpers reused across services: the filter/sort/pagination builders and the user-friendly exception type. *(Infrastructure layer)*
- **Hubs/** - SignalR hubs that push live updates (e.g. order status) to connected clients. *(Presentation layer)*
- **Workers/** - background hosted services that run on their own schedule: index maintenance, the email queue, and the weekly report. *(Infrastructure layer)*
- **Reports/** - weekly audit-report generation and delivery: HTML/SMS rendering, the send queue, and the email/SMS senders. *(Application layer)*
- **Migrations/** - EF Core migration files. Auto-generated when you run `dotnet ef migrations add`. Each file is a snapshot of a schema change - never edit manually. *(Infrastructure layer)*
- **wwwroot/** - files served statically over HTTP, including user uploads (product images, order invoices).
- **appsettings.json** - app configuration. Holds environment-specific values like connection strings and feature flags. Never hardcode these in code, place them here.
- **appsettings.Development.json** - local overrides for development. Gitignored - contains secrets like passwords. Automatically merged on top of `appsettings.json` when running locally.
- **Program.cs** - startup wiring. Registers services, middleware, background workers, and CORS so the app knows how to run. *(Composition root)*


### ⚠ Ignore - auto-generated or scaffolding

- **bin/** - compiled output produced by `dotnet build`. Never edit.
- **obj/** - intermediate build files produced by `dotnet build`. Never edit.
- **.config/** - local .NET tool manifest (e.g. `dotnet-ef`). Managed via `dotnet tool` commands.
- **Properties/** - dev launch profiles (port, environment). Only relevant if you want to run local debugging (breakpoints, etc)
- **ProductApi.csproj** - project definition file. Managed by the framework - only touch it to add/remove NuGet packages.


## Database Architecture

- **Engine**: PostgreSQL 18 - runs locally, installed on the developer's machine.
- **ORM**: Entity Framework Core 8 with Npgsql driver - translates C# LINQ queries into SQL, no raw SQL needed for basic operations.
- **DbContext** (`Data/AppDbContext.cs`) - the single entry point to the database. Exposes tables as `DbSet<T>` collections you query like C# lists.
- **Migrations** (`Migrations/`) - version control for your database schema. Every schema change is a new migration file. Run `dotnet ef migrations add <Name>` to create one, `dotnet ef database update` to apply it.
- **Connection string** - stored in `appsettings.Development.json` (gitignored). Never committed. In production, injected via environment variables.

### Tables

| Table | Description |
|---|---|
| `Products` | Product catalog - name, category, price, stock, image path, version |
| `Users` | Accounts - name, email, phone, role, feature flags, auth/2FA fields, report channel, version |
| `Orders` | Orders - user, product, quantity, total, status, AWB, invoice path, version |
| `AuditLogs` | Append-only audit trail - before/after JSONB snapshots written by DB triggers on every change |
| `mv_weekly_audit_report` | Materialized view - last week's insert/update/delete counts per audited table |
| `__EFMigrationsHistory` | EF Core internal - tracks which migrations have been applied. Never touch it. |


## Frontend Architecture

### ✓ Important

- **api/** - transport + API layers. The shared HTTP client (`fetch`, auth header, error parsing) and one typed module per resource. Nothing else touches `fetch` directly.
- **hooks/** - service layer. Data fetching, pagination, and mutations per resource. Components never call the API directly - they go through hooks.
- **pages/** - presentation layer. One screen per route (products, users, orders, profile, auth). Compose components and hooks.
- **components/** - presentation layer. Reusable UI pieces (tables, forms, modals) shared across pages.
- **context/** - React context providers for app-wide state (auth session, theme).
- **routes/** - route definitions and guards wiring pages to URLs.
- **auth/** - client-side session handling (token storage/retrieval).
- **types/** - domain types. TypeScript interfaces mirroring the API contracts. The shape of data across the whole app.
- **constants/** - shared constants (pagination defaults, UI labels).
- **locales/** - i18n translation dictionaries (one JSON per language).
- **utils/** - small pure helpers (date formatting, etc.).
- **App.tsx** - UI orchestration. Top-level layout and provider/route composition.
- **main.tsx** - composition root. Entry point - mounts the React app into `index.html`.
- **i18n.ts** - i18next setup; loads `locales/` and picks the active language.

### ⚠ Ignore - boilerplate

- **vite-env.d.ts** - tells TypeScript about Vite's `import.meta.env`. Auto-generated, never edit.
- **index.css / themes.css** - global styles and theme variables. Edit for styling, not structure.
- **index.html** - shell HTML. Vite injects the built JS bundle here. Only touch it to add a favicon or meta tags.
- **package.json** - npm dependencies and scripts. Touch it only to add/remove packages.
- **vite.config.ts** - Vite bundler config. Touch it only to change build settings.
- **tsconfig.json** - TypeScript compiler config. Touch it only to change TS strictness rules.


## Shared

- **.gitignore** - tells Git which files not to track. Excludes auto-generated folders (`bin/`, `obj/`, `node_modules/`, `dist/`) - these can be hundreds of megabytes and are always rebuilt locally, so committing them would bloat the repository for no reason. If you ever randomly see 10.000 file changes in your git commit, this is likely the culprit.
- **ProjectArchitecture.md** - this file. Project documentation for anyone new to the codebase.
- **README.md** - quick-start and project overview.
- **.claude/** - Claude Code configuration. Contains project-specific instructions for the AI assistant.

---

## Folder Structure

```
Builder/
├── backend/
│   ├── Configuration/              # Infrastructure layer - typed settings - ✓ important
│   ├── Constants/                  # Shared layer - constants & defaults - ✓ important
│   ├── Contracts/                  # Application layer - DTOs - ✓ important
│   ├── Controllers/                # Presentation layer - ✓ important
│   ├── Data/                       # Infrastructure layer - DbContext, seeder - ✓ important
│   ├── Hubs/                       # Presentation layer - SignalR - ✓ important
│   ├── Infrastructure/             # Infrastructure layer - shared helpers - ✓ important
│   ├── Migrations/                 # EF Core schema history - ✓ important
│   ├── Models/                     # Domain layer - entities & read models - ✓ important
│   ├── Reports/                    # Application layer - report generation - ✓ important
│   ├── Services/                   # Application layer - business logic - ✓ important
│   ├── Workers/                    # Infrastructure layer - background jobs - ✓ important
│   ├── wwwroot/                    # Served static files & uploads - ✓ important
│   ├── .config/                    # .NET tool manifest - ⚠ ignore
│   ├── bin/  obj/                  # Auto-generated build output - ⚠ ignore
│   ├── Properties/                 # Dev launch profiles - ⚠ ignore
│   ├── appsettings.json            # App configuration - ✓ important
│   ├── appsettings.Development.json# Local secrets (gitignored) - ✓ important
│   ├── ProductApi.csproj           # Project definition - ⚠ ignore
│   └── Program.cs                  # Composition root - ✓ important
│
└── frontend/
    ├── src/
    │   ├── api/            # Transport + API layers - HTTP client, typed calls
    │   ├── hooks/          # Service layer - data fetching, pagination, mutations
    │   ├── pages/          # Presentation layer - one screen per route
    │   ├── components/     # Presentation layer - reusable UI pieces
    │   ├── context/        # App-wide state providers (auth, theme)
    │   ├── routes/         # Route definitions & guards
    │   ├── auth/           # Client-side session/token handling
    │   ├── types/          # TypeScript domain types
    │   ├── constants/      # Shared constants & labels
    │   ├── locales/        # i18n translation dictionaries
    │   ├── utils/          # Pure helpers
    │   ├── App.tsx         # UI orchestration
    │   ├── main.tsx        # Composition root
    │   └── i18n.ts         # i18next setup
    ├── index.html          # Shell HTML - Vite injects the built JS here
    ├── package.json        # npm dependencies and scripts
    ├── vite.config.ts      # Vite bundler config
    └── tsconfig.json       # TypeScript compiler config
```
